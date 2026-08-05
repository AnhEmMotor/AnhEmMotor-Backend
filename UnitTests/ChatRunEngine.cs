using Application.DTOs.Chat;
using Application.Features.ManagerChat.Commands.CancelChatRun;
using Application.Features.ManagerChat.Commands.StartChatRun;
using Application.Features.ManagerChat.Queries.GetActiveChatRun;
using Application.Features.ManagerChat.Queries.GetChatRunEvents;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class ChatRunEngine
{
    private readonly Mock<IChatReadRepository> _chatRead = new();
    private readonly Mock<IChatInsertRepository> _chatInsert = new();
    private readonly Mock<IPermissionReadRepository> _permissions = new();
    private readonly Mock<IChatRunQueue> _queue = new();
    private readonly Mock<IChatRunTokenStore> _tokenStore = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private StartChatRunCommandHandler CreateStartHandler() =>
        new(_chatRead.Object, _chatInsert.Object, _permissions.Object,
            _queue.Object, _tokenStore.Object, _unitOfWork.Object);

    private void GivenSessionOwnedBy(Guid userId, Guid sessionId)
    {
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = userId });
        _chatRead.Setup(x => x.GetActiveRunForUserAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ChatRun?)null);
    }

    // ---- StartChatRunCommandHandler ----

    [Fact(DisplayName = "START_01 - Không có quyền thì ném UnauthorizedAccessException")]
    public async Task Start_ThrowsUnauthorized_WhenNoPermission()
    {
        var userId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        var handler = CreateStartHandler();
        var command = new StartChatRunCommand(Guid.NewGuid(), "xin chào", userId, "token");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _queue.Verify(x => x.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "START_02 - Session của người khác thì bị từ chối")]
    public async Task Start_Throws_WhenSessionBelongsToAnotherUser()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = Guid.NewGuid() });

        var handler = CreateStartHandler();
        var command = new StartChatRunCommand(sessionId, "xin chào", userId, "token");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "START_03 - Đã có run đang chạy thì từ chối tạo run mới (1 run/user)")]
    public async Task Start_Throws_WhenUserAlreadyHasActiveRun()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = userId });
        _chatRead.Setup(x => x.GetActiveRunForUserAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun { Id = Guid.NewGuid(), SessionId = sessionId, Status = ChatRunStatus.Running });

        var handler = CreateStartHandler();
        var command = new StartChatRunCommand(sessionId, "xin chào", userId, "token");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _queue.Verify(x => x.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "START_04 - Tạo run thành công: lưu tin nhắn, lưu token, enqueue và trả runId ngay")]
    public async Task Start_CreatesRun_SavesTokenAndEnqueues()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenSessionOwnedBy(userId, sessionId);

        ChatMessage? savedMessage = null;
        ChatRun? savedRun = null;
        _chatInsert.Setup(x => x.AddMessage(It.IsAny<ChatMessage>())).Callback<ChatMessage>(m => savedMessage = m);
        _chatInsert.Setup(x => x.AddRun(It.IsAny<ChatRun>())).Callback<ChatRun>(r => savedRun = r);

        var handler = CreateStartHandler();
        var command = new StartChatRunCommand(sessionId, "doanh thu tháng này?", userId, "user-jwt-token");

        var runId = await handler.Handle(command, CancellationToken.None);

        runId.Should().NotBeEmpty();
        savedMessage.Should().NotBeNull();
        savedMessage!.Role.Should().Be(ChatRole.User);
        savedMessage.Message.Should().Be("doanh thu tháng này?");

        savedRun.Should().NotBeNull();
        savedRun!.Id.Should().Be(runId);
        savedRun.Status.Should().Be(ChatRunStatus.Pending);
        savedRun.UserMessage.Should().Be("doanh thu tháng này?");

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tokenStore.Verify(x => x.Store(runId, "user-jwt-token"), Times.Once);
        _queue.Verify(x => x.EnqueueAsync(runId, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---- CancelChatRunCommandHandler ----

    [Fact(DisplayName = "CANCEL_01 - Không có quyền thì ném UnauthorizedAccessException")]
    public async Task Cancel_ThrowsUnauthorized_WhenNoPermission()
    {
        var userId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        var cancellationRegistry = new Mock<IChatRunCancellationRegistry>();
        var sidecarStreamClient = new Mock<ISidecarStreamClient>();
        var handler = new CancelChatRunCommandHandler(
            _chatRead.Object, _permissions.Object, cancellationRegistry.Object, sidecarStreamClient.Object);

        var act = async () => await handler.Handle(new CancelChatRunCommand(Guid.NewGuid(), userId), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "CANCEL_02 - Run của người khác thì bị từ chối")]
    public async Task Cancel_Throws_WhenRunBelongsToAnotherUser()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun
                 {
                     Id = runId,
                     Session = new ChatSession { Id = Guid.NewGuid(), UserId = Guid.NewGuid() }
                 });

        var cancellationRegistry = new Mock<IChatRunCancellationRegistry>();
        var sidecarStreamClient = new Mock<ISidecarStreamClient>();
        var handler = new CancelChatRunCommandHandler(
            _chatRead.Object, _permissions.Object, cancellationRegistry.Object, sidecarStreamClient.Object);

        var act = async () => await handler.Handle(new CancelChatRunCommand(runId, userId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        cancellationRegistry.Verify(x => x.TryCancel(It.IsAny<Guid>()), Times.Never);
    }

    [Fact(DisplayName = "CANCEL_03 - Huỷ thành công: báo sidecar và huỷ CancellationToken cục bộ")]
    public async Task Cancel_NotifiesSidecarAndCancelsLocalToken()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun
                 {
                     Id = runId,
                     Session = new ChatSession { Id = Guid.NewGuid(), UserId = userId }
                 });

        var cancellationRegistry = new Mock<IChatRunCancellationRegistry>();
        var sidecarStreamClient = new Mock<ISidecarStreamClient>();
        var handler = new CancelChatRunCommandHandler(
            _chatRead.Object, _permissions.Object, cancellationRegistry.Object, sidecarStreamClient.Object);

        await handler.Handle(new CancelChatRunCommand(runId, userId), CancellationToken.None);

        sidecarStreamClient.Verify(x => x.CancelAsync(runId, It.IsAny<CancellationToken>()), Times.Once);
        cancellationRegistry.Verify(x => x.TryCancel(runId), Times.Once);
    }

    // ---- GetActiveChatRunQueryHandler ----

    [Fact(DisplayName = "ACTIVE_01 - Không có run đang chạy thì trả về null")]
    public async Task GetActiveRun_ReturnsNull_WhenNoActiveRun()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = userId });
        _chatRead.Setup(x => x.GetActiveRunForUserAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ChatRun?)null);

        var handler = new GetActiveChatRunQueryHandler(_chatRead.Object, _permissions.Object, currentUser.Object);

        var result = await handler.Handle(new GetActiveChatRunQuery(sessionId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact(DisplayName = "ACTIVE_02 - Có run đang chạy thì trả về đủ thông tin để khôi phục")]
    public async Task GetActiveRun_ReturnsDto_WhenRunIsActive()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = userId });
        _chatRead.Setup(x => x.GetActiveRunForUserAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun
                 {
                     Id = runId,
                     SessionId = sessionId,
                     Status = ChatRunStatus.Running,
                     LastSeq = 7,
                     UserMessage = "doanh thu tháng này?",
                     PartialOutput = "Doanh thu tháng 7 hiện đạt "
                 });

        var handler = new GetActiveChatRunQueryHandler(_chatRead.Object, _permissions.Object, currentUser.Object);

        var result = await handler.Handle(new GetActiveChatRunQuery(sessionId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.RunId.Should().Be(runId);
        result.Value.LastSeq.Should().Be(7);
        result.Value.PartialOutput.Should().Be("Doanh thu tháng 7 hiện đạt ");
    }

    [Fact(DisplayName = "ACTIVE_03 - Run đang chạy thuộc session khác thì không trả về")]
    public async Task GetActiveRun_ReturnsNull_WhenActiveRunBelongsToAnotherSession()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var otherSessionId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = userId });
        _chatRead.Setup(x => x.GetActiveRunForUserAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun { Id = Guid.NewGuid(), SessionId = otherSessionId, Status = ChatRunStatus.Running });

        var handler = new GetActiveChatRunQueryHandler(_chatRead.Object, _permissions.Object, currentUser.Object);

        var result = await handler.Handle(new GetActiveChatRunQuery(sessionId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ---- GetChatRunEventsQueryHandler ----

    [Fact(DisplayName = "EVENTS_01 - Trả về event theo Seq tăng dần kèm cờ RunIsTerminal")]
    public async Task GetRunEvents_ReturnsEventsAfterSeq_AndTerminalFlag()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun
                 {
                     Id = runId,
                     Status = ChatRunStatus.Completed,
                     Session = new ChatSession { Id = Guid.NewGuid(), UserId = userId }
                 });
        _chatRead.Setup(x => x.GetRunEventsAsync(runId, 2, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([
                     new ChatRunEvent { RunId = runId, Seq = 3, Type = ChatRunEventType.TextDelta, Payload = "abc" },
                     new ChatRunEvent { RunId = runId, Seq = 4, Type = ChatRunEventType.RunCompleted, Payload = "" },
                 ]);

        var handler = new GetChatRunEventsQueryHandler(_chatRead.Object, _permissions.Object, currentUser.Object);

        var result = await handler.Handle(new GetChatRunEventsQuery(runId, 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RunIsTerminal.Should().BeTrue();
        result.Value.Events.Should().HaveCount(2);
        result.Value.Events.Select(e => e.Seq).Should().ContainInOrder(3, 4);
    }

    [Fact(DisplayName = "EVENTS_02 - Run của người khác thì bị từ chối")]
    public async Task GetRunEvents_Fails_WhenRunBelongsToAnotherUser()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun
                 {
                     Id = runId,
                     Session = new ChatSession { Id = Guid.NewGuid(), UserId = Guid.NewGuid() }
                 });

        var handler = new GetChatRunEventsQueryHandler(_chatRead.Object, _permissions.Object, currentUser.Object);

        var result = await handler.Handle(new GetChatRunEventsQuery(runId, 0), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact(DisplayName = "EVENTS_03 - Run vẫn đang chạy thì RunIsTerminal là false")]
    public async Task GetRunEvents_RunIsTerminalFalse_WhenRunStillRunning()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetRunByIdAsync(runId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatRun
                 {
                     Id = runId,
                     Status = ChatRunStatus.Running,
                     Session = new ChatSession { Id = Guid.NewGuid(), UserId = userId }
                 });
        _chatRead.Setup(x => x.GetRunEventsAsync(runId, 0, It.IsAny<CancellationToken>()))
                 .ReturnsAsync([]);

        var handler = new GetChatRunEventsQueryHandler(_chatRead.Object, _permissions.Object, currentUser.Object);

        var result = await handler.Handle(new GetChatRunEventsQuery(runId, 0), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RunIsTerminal.Should().BeFalse();
    }
}
