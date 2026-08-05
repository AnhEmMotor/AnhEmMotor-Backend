using Application.DTOs.Chat;
using Application.Features.ManagerChat.Commands.SendSteeringMessage;
using Application.Features.ManagerChat.Commands.StartChatRun;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;

namespace UnitTests;

public class ManagerChatSteering
{
    // ---- SteeringClassifier (Tầng 1 - luật) ----

    [Theory(DisplayName = "CLASSIFY_01 - Bảng phân loại theo luật")]
    [InlineData("À nhầm, tháng trước cơ", ChatSteeringMode.Interrupt)]
    [InlineData("thêm cả số đơn hàng nữa", null)]
    [InlineData("dừng", ChatSteeringMode.Restart)]
    [InlineData("stop", ChatSteeringMode.Restart)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    public void Classify_TraVeDungCheDo(string text, string? expected)
    {
        SteeringClassifier.Classify(text).Should().Be(expected);
    }

    // ---- SendSteeringMessageCommandHandler ----

    private readonly Mock<IChatReadRepository> _chatRead = new();
    private readonly Mock<IChatInsertRepository> _chatInsert = new();
    private readonly Mock<IPermissionReadRepository> _permissions = new();
    private readonly Mock<IChatRunWriter> _writer = new();
    private readonly Mock<IChatRunCancellationRegistry> _cancellationRegistry = new();
    private readonly Mock<ISidecarStreamClient> _sidecarStreamClient = new();
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private SendSteeringMessageCommandHandler CreateHandler() =>
        new(_chatRead.Object, _chatInsert.Object, _permissions.Object, _writer.Object,
            _cancellationRegistry.Object, _sidecarStreamClient.Object, _sender.Object, _unitOfWork.Object);

    private void GivenActiveRun(Guid userId, Guid runId, Guid sessionId, string status = ChatRunStatus.Running)
    {
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _chatRead.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun
                 {
                     Id = runId,
                     SessionId = sessionId,
                     Status = status,
                     Session = new ChatSession { Id = sessionId, UserId = userId },
                 });
        _chatRead.Setup(x => x.CountSteeringMessagesAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(0);
    }

    [Fact(DisplayName = "STEER_01 - Không có quyền thì trả về Forbidden")]
    public async Task Handle_ReturnsForbidden_WhenNoPermission()
    {
        var userId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendSteeringMessageCommand(Guid.NewGuid(), "à nhầm", userId, "token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Forbidden");
    }

    [Fact(DisplayName = "STEER_02 - Run của người khác thì trả về NotFound")]
    public async Task Handle_ReturnsNotFound_WhenRunBelongsToAnotherUser()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _chatRead.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun { Id = runId, Session = new ChatSession { Id = Guid.NewGuid(), UserId = Guid.NewGuid() } });

        var handler = CreateHandler();
        var result = await handler.Handle(new SendSteeringMessageCommand(runId, "à nhầm", userId, "token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact(DisplayName = "STEER_03 - Run vừa kết thúc thì tự tạo run mới, không báo lỗi")]
    public async Task Handle_StartsNewRun_WhenRunAlreadyTerminal()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var newRunId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _chatRead.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun
                 {
                     Id = runId,
                     SessionId = sessionId,
                     Status = ChatRunStatus.Completed,
                     Session = new ChatSession { Id = sessionId, UserId = userId },
                 });
        _sender.Setup(x => x.Send(It.IsAny<StartChatRunCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(newRunId);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendSteeringMessageCommand(runId, "tháng trước", userId, "token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RunId.Should().Be(newRunId);
        result.Value.Mode.Should().Be(ChatSteeringMode.Restart);
        _writer.Verify(x => x.AppendPendingSteeringAsync(It.IsAny<Guid>(), It.IsAny<SteeringQueueItem>(), It.IsAny<int>()), Times.Never);
    }

    [Fact(DisplayName = "STEER_04 - Đã gửi đủ 5 steering thì từ chối, không lưu tin nhắn mới")]
    public async Task Handle_RejectsQueueMode_WhenAlreadyAtSteeringLimit()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenActiveRun(userId, runId, sessionId);
        _chatRead.Setup(x => x.CountSteeringMessagesAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(5);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendSteeringMessageCommand(runId, "thêm nữa", userId, "token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
        _chatInsert.Verify(x => x.AddMessage(It.IsAny<ChatMessage>()), Times.Never);
    }

    [Fact(DisplayName = "STEER_05 - Đính chính hợp lệ được lưu và thêm vào hàng chờ")]
    public async Task Handle_AppendsToQueue_WhenInterruptMessageSent()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenActiveRun(userId, runId, sessionId);
        _writer.Setup(x => x.AppendPendingSteeringAsync(runId, It.IsAny<SteeringQueueItem>(), It.IsAny<int>()))
               .ReturnsAsync(PendingSteeringAppendResult.Appended);

        ChatMessage? saved = null;
        _chatInsert.Setup(x => x.AddMessage(It.IsAny<ChatMessage>())).Callback<ChatMessage>(m => saved = m);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendSteeringMessageCommand(runId, "à nhầm, tháng trước cơ", userId, "token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Mode.Should().Be(ChatSteeringMode.Interrupt);
        result.Value.RunId.Should().Be(runId);

        saved.Should().NotBeNull();
        saved!.IsSteering.Should().BeTrue();
        saved.RunId.Should().Be(runId);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _writer.Verify(x => x.AppendAsync(runId, ChatRunEventType.SteeringReceived, It.IsAny<object>()), Times.Once);
    }

    [Fact(DisplayName = "STEER_06 - Race: run vừa kết thúc đúng lúc ghi hàng chờ thì tự tạo run mới")]
    public async Task Handle_StartsNewRun_WhenAppendRacesWithRunEnding()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var newRunId = Guid.NewGuid();
        GivenActiveRun(userId, runId, sessionId);
        _writer.Setup(x => x.AppendPendingSteeringAsync(runId, It.IsAny<SteeringQueueItem>(), It.IsAny<int>()))
               .ReturnsAsync(PendingSteeringAppendResult.RunNotActive);
        _sender.Setup(x => x.Send(It.IsAny<StartChatRunCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(newRunId);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendSteeringMessageCommand(runId, "thêm nữa", userId, "token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RunId.Should().Be(newRunId);
        result.Value.Mode.Should().Be(ChatSteeringMode.Restart);
    }

    [Fact(DisplayName = "STEER_07 - Hàng chờ đầy (TooMany) thì trả lỗi có hướng dẫn")]
    public async Task Handle_ReturnsValidationError_WhenQueueIsFull()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenActiveRun(userId, runId, sessionId);
        _writer.Setup(x => x.AppendPendingSteeringAsync(runId, It.IsAny<SteeringQueueItem>(), It.IsAny<int>()))
               .ReturnsAsync(PendingSteeringAppendResult.TooMany);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendSteeringMessageCommand(runId, "thêm nữa", userId, "token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
    }

    [Fact(DisplayName = "STEER_08 - Restart: huỷ sidecar/registry rồi khởi động run mới")]
    public async Task Handle_CancelsAndStartsNewRun_OnRestartMode()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var newRunId = Guid.NewGuid();
        GivenActiveRun(userId, runId, sessionId);
        _sender.Setup(x => x.Send(It.IsAny<StartChatRunCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(newRunId);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendSteeringMessageCommand(runId, "dừng", userId, "token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RunId.Should().Be(newRunId);
        result.Value.Mode.Should().Be(ChatSteeringMode.Restart);
        _sidecarStreamClient.Verify(x => x.CancelAsync(runId, It.IsAny<CancellationToken>()), Times.Once);
        _cancellationRegistry.Verify(x => x.TryCancel(runId), Times.Once);
        _writer.Verify(x => x.AppendPendingSteeringAsync(It.IsAny<Guid>(), It.IsAny<SteeringQueueItem>(), It.IsAny<int>()), Times.Never);
    }

    [Fact(DisplayName = "STEER_09 - Restart: run cũ chưa kịp huỷ thì thử lại đến khi thành công")]
    public async Task Handle_RetriesStartRun_WhenOldRunNotYetCancelled()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var newRunId = Guid.NewGuid();
        GivenActiveRun(userId, runId, sessionId);

        var callCount = 0;
        _sender.Setup(x => x.Send(It.IsAny<StartChatRunCommand>(), It.IsAny<CancellationToken>()))
               .Returns(() =>
               {
                   callCount++;
                   if (callCount < 3) throw new InvalidOperationException("Đang có một tiến trình AI khác đang chạy.");
                   return Task.FromResult(newRunId);
               });

        var handler = CreateHandler();
        var result = await handler.Handle(new SendSteeringMessageCommand(runId, "huỷ", userId, "token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RunId.Should().Be(newRunId);
        callCount.Should().Be(3);
    }
}
