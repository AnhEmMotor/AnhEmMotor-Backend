using Application.ApiContracts.ManagerChat.Requests;
using Application.Common.Models;
using Application.DTOs.Chat;
using Application.Features.ManagerChat.Commands.ApproveChatPlan;
using Application.Features.ManagerChat.Commands.RejectChatPlan;
using Application.Features.ManagerChat.Commands.SendPlanChatMessage;
using Application.Features.ManagerChat.Commands.UpdateChatPlan;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;
using System.Text.Json;

namespace UnitTests;

public class ManagerChatPlan
{
    private readonly Mock<IChatReadRepository> _chatRead = new();
    private readonly Mock<IChatUpdateRepository> _chatUpdate = new();
    private readonly Mock<IChatRunWriter> _writer = new();
    private readonly Mock<ICurrentUserContext> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPermissionReadRepository> _permissions = new();
    private readonly Mock<IChatRunTokenStore> _tokenStore = new();
    private readonly Mock<IChatRunQueue> _runQueue = new();
    private readonly Mock<ISidecarStreamClient> _sidecarStreamClient = new();
    private readonly Mock<IChatInsertRepository> _chatInsert = new();
    private readonly Mock<ISender> _sender = new();

    private static ChatPlan GivenPlan(Guid runId, Guid userId, string status, List<PlanStepDto> steps, int version = 1) => new(
        )
    {
        Id = Guid.NewGuid(),
        RunId = runId,
        SessionId = Guid.NewGuid(),
        Version = version,
        Status = status,
        Steps = JsonSerializer.Serialize(steps),
        Run = new ChatRun { Id = runId, Session = new ChatSession { Id = Guid.NewGuid(), UserId = userId } },
    };

    private static PlanStepDto Step(
        string id,
        int order,
        string status = PlanStepStatus.Pending,
        bool editedByUser = false,
        List<string>? tools = null) => new(
        id,
        order,
        $"Bước {order}",
        "chi tiết",
        tools ?? [],
        status,
        editedByUser,
        null);

    private UpdateChatPlanCommandHandler CreateUpdateHandler() => new(
        _chatRead.Object,
        _chatUpdate.Object,
        _writer.Object,
        _currentUser.Object,
        _unitOfWork.Object);

    [Fact(DisplayName = "PLAN_UPDATE_01 - Version lệch thì trả Conflict, plan không đổi")]
    public async Task Update_ReturnsConflict_WhenVersionMismatch()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Drafting, [Step("s1", 1)], version: 3);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateUpdateHandler();
        var result = await handler.Handle(
            new UpdateChatPlanCommand(
                runId,
                1,
                [new UpdatePlanStepOperation { Type = "edit", StepId = "s1", Title = "Sửa" }]),
            CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Conflict");
        plan.Version.Should().Be(3);
        _chatUpdate.Verify(x => x.UpdatePlan(It.IsAny<ChatPlan>()), Times.Never);
    }

    [Fact(DisplayName = "PLAN_UPDATE_02 - Xoá bước: status=skipped, giữ nguyên id, không mất khỏi mảng")]
    public async Task Update_MarksStepSkipped_WhenRemoved()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1), Step("s2", 2)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateUpdateHandler();
        var result = await handler.Handle(
            new UpdateChatPlanCommand(runId, 1, [new UpdatePlanStepOperation { Type = "remove", StepId = "s1" }]),
            CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Steps.Should().HaveCount(2);
        var removed = result.Value.Steps.Single(s => s.Id == "s1");
        removed.Status.Should().Be(PlanStepStatus.Skipped);
        removed.EditedByUser.Should().BeTrue();
        result.Value.Version.Should().Be(2);
    }

    [Fact(DisplayName = "PLAN_UPDATE_03 - Sửa bước đang running/done thì bị từ chối")]
    public async Task Update_Rejects_WhenEditingRunningOrDoneStep()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Executing, [Step("s1", 1, status: PlanStepStatus.Running)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateUpdateHandler();
        var result = await handler.Handle(
            new UpdateChatPlanCommand(
                runId,
                1,
                [new UpdatePlanStepOperation { Type = "edit", StepId = "s1", Title = "x" }]),
            CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
    }

    [Fact(DisplayName = "PLAN_UPDATE_04 - Quá 8 bước (không tính skipped) thì bị từ chối")]
    public async Task Update_Rejects_WhenExceedingMaxSteps()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var steps = Enumerable.Range(1, 8).Select(i => Step($"s{i}", i)).ToList();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Drafting, steps);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateUpdateHandler();
        var result = await handler.Handle(
            new UpdateChatPlanCommand(
                runId,
                1,
                [new UpdatePlanStepOperation { Type = "add", Title = "Bước 9", Detail = "d" }]),
            CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
    }

    [Fact(DisplayName = "PLAN_UPDATE_05 - Plan vẫn Ready nhưng sửa đúng bước đang running thì bị từ chối")]
    public async Task Update_Rejects_WhenEditingStepStatusRunning_WhilePlanReady()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1, status: PlanStepStatus.Running)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateUpdateHandler();
        var result = await handler.Handle(
            new UpdateChatPlanCommand(
                runId,
                1,
                [new UpdatePlanStepOperation { Type = "edit", StepId = "s1", Title = "x" }]),
            CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
        _chatUpdate.Verify(x => x.UpdatePlan(It.IsAny<ChatPlan>()), Times.Never);
    }

    [Fact(DisplayName = "PLAN_UPDATE_06 - Plan vẫn Ready nhưng xoá đúng bước đã done thì bị từ chối")]
    public async Task Update_Rejects_WhenRemovingStepStatusDone_WhilePlanReady()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1, status: PlanStepStatus.Done)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateUpdateHandler();
        var result = await handler.Handle(
            new UpdateChatPlanCommand(runId, 1, [new UpdatePlanStepOperation { Type = "remove", StepId = "s1" }]),
            CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
        _chatUpdate.Verify(x => x.UpdatePlan(It.IsAny<ChatPlan>()), Times.Never);
    }

    [Fact(DisplayName = "PLAN_UPDATE_07 - Bình luận vào 1 bước: thêm đúng vào mảng comments, giữ bình luận cũ")]
    public async Task Update_AppendsComment_ToStep()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateUpdateHandler();
        var result = await handler.Handle(
            new UpdateChatPlanCommand(
                runId,
                1,
                [new UpdatePlanStepOperation { Type = "comment", StepId = "s1", Comment = "Bình luận 1" }]),
            CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        var step = result.Value.Steps.Single(s => s.Id == "s1");
        step.Comments.Should().ContainSingle(c => c.Text == "Bình luận 1");
        step.EditedByUser.Should().BeTrue();
        var result2 = await handler.Handle(
            new UpdateChatPlanCommand(
                runId,
                2,
                [new UpdatePlanStepOperation { Type = "comment", StepId = "s1", Comment = "Bình luận 2" }]),
            CancellationToken.None);
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Steps.Single(s => s.Id == "s1").Comments.Should().HaveCount(2);
    }

    [Fact(DisplayName = "PLAN_UPDATE_08 - Bình luận rỗng thì bị từ chối")]
    public async Task Update_Rejects_WhenCommentIsEmpty()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateUpdateHandler();
        var result = await handler.Handle(
            new UpdateChatPlanCommand(
                runId,
                1,
                [new UpdatePlanStepOperation { Type = "comment", StepId = "s1", Comment = "   " }]),
            CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
    }

    private ApproveChatPlanCommandHandler CreateApproveHandler() => new(
        _chatRead.Object,
        _chatUpdate.Object,
        _permissions.Object,
        _writer.Object,
        _tokenStore.Object,
        _runQueue.Object,
        _sidecarStreamClient.Object,
        _currentUser.Object,
        _unitOfWork.Object);

    [Fact(DisplayName = "PLAN_APPROVE_01 - Duyệt plan của người khác thì NotFound")]
    public async Task Approve_ReturnsNotFound_WhenPlanBelongsToAnotherUser()
    {
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, Guid.NewGuid(), ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateApproveHandler();
        var result = await handler.Handle(new ApproveChatPlanCommand(runId, 1), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact(DisplayName = "PLAN_APPROVE_02 - Duyệt khi Status=Drafting thì bị từ chối")]
    public async Task Approve_Rejects_WhenStatusIsDrafting()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Drafting, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateApproveHandler();
        var result = await handler.Handle(new ApproveChatPlanCommand(runId, 1), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
        _runQueue.Verify(x => x.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(
        DisplayName = "PLAN_APPROVE_03 - Tool trong plan đã bị gỡ: quay về Drafting, đánh dấu Invalid, không thực thi")]
    public async Task Approve_InvalidatesPlan_WhenToolRemoved()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(
            runId,
            userId,
            ChatPlanStatus.Ready,
            [Step("s1", 1, tools: ["get_stock"]), Step("s2", 2, tools: ["get_orders"])]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetAccessToken()).Returns("fresh-token");
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sidecarStreamClient.Setup(
            x => x.RevalidatePlanAsync(
                runId,
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanRevalidationResult(false, ["get_stock"]));
        var handler = CreateApproveHandler();
        var result = await handler.Handle(new ApproveChatPlanCommand(runId, 1), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        plan.Status.Should().Be(ChatPlanStatus.Drafting);
        var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps)!;
        steps.Single(s => s.Id == "s1").Status.Should().Be(PlanStepStatus.Invalid);
        steps.Single(s => s.Id == "s2").Status.Should().Be(PlanStepStatus.Pending);
        _runQueue.Verify(x => x.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenStore.Verify(x => x.Store(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _writer.Verify(x => x.AppendAsync(runId, ChatRunEventType.PlanInvalidated, It.IsAny<object>()), Times.Once);
    }

    [Fact(DisplayName = "PLAN_APPROVE_04 - Duyệt thành công: cấp token mới, chuyển Executing, enqueue lại run")]
    public async Task Approve_Succeeds_MintsFreshTokenAndEnqueues()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1, tools: ["get_stock"])]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetAccessToken()).Returns("fresh-token");
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sidecarStreamClient.Setup(
            x => x.RevalidatePlanAsync(
                runId,
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanRevalidationResult(true, []));
        var handler = CreateApproveHandler();
        var result = await handler.Handle(new ApproveChatPlanCommand(runId, 1), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        plan.Status.Should().Be(ChatPlanStatus.Executing);
        _tokenStore.Verify(x => x.Store(runId, "fresh-token"), Times.Once);
        _runQueue.Verify(x => x.EnqueueAsync(runId, It.IsAny<CancellationToken>()), Times.Once);
        _writer.Verify(x => x.AppendAsync(runId, ChatRunEventType.PlanApproved, It.IsAny<object>()), Times.Once);
    }

    [Fact(
        DisplayName = "PLAN_APPROVE_05 - Không còn permission tại thời điểm duyệt thì Forbidden, không dùng bản chụp cũ")]
    public async Task Approve_ReturnsForbidden_WhenPermissionRevokedSinceCreated()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = CreateApproveHandler();
        var result = await handler.Handle(new ApproveChatPlanCommand(runId, 1), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Forbidden");
        _sidecarStreamClient.Verify(
            x => x.RevalidatePlanAsync(
                It.IsAny<Guid>(),
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private RejectChatPlanCommandHandler CreateRejectHandler() => new(
        _chatRead.Object,
        _chatUpdate.Object,
        _writer.Object,
        _currentUser.Object,
        _unitOfWork.Object);

    [Fact(DisplayName = "PLAN_REJECT_01 - Huỷ plan của người khác thì NotFound")]
    public async Task Reject_ReturnsNotFound_WhenPlanBelongsToAnotherUser()
    {
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, Guid.NewGuid(), ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateRejectHandler();
        var result = await handler.Handle(new RejectChatPlanCommand(runId), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact(DisplayName = "PLAN_REJECT_02 - Huỷ plan đã Executing thì bị từ chối")]
    public async Task Reject_Rejects_WhenAlreadyExecuting()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Executing, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateRejectHandler();
        var result = await handler.Handle(new RejectChatPlanCommand(runId), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
    }

    [Fact(DisplayName = "PLAN_REJECT_03 - Huỷ hợp lệ: plan Rejected, run Cancelled")]
    public async Task Reject_Succeeds_WhenPending()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateRejectHandler();
        var result = await handler.Handle(new RejectChatPlanCommand(runId), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        plan.Status.Should().Be(ChatPlanStatus.Rejected);
        _writer.Verify(x => x.CancelAsync(runId, string.Empty, It.IsAny<DateTime>()), Times.Once);
    }

    private SendPlanChatMessageCommandHandler CreateSendPlanChatHandler() => new(
        _chatRead.Object,
        _chatInsert.Object,
        _permissions.Object,
        _sidecarStreamClient.Object,
        _currentUser.Object,
        _unitOfWork.Object,
        _sender.Object);

    private void GivenHasPermission(Guid userId) => _permissions.Setup(
        x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    [Fact(DisplayName = "PLAN_CHAT_01 - Gõ 'duyệt' thì gọi ApproveChatPlanCommand, không cần LLM")]
    public async Task SendPlanChat_RoutesToApprove_WhenTextMatchesApproveKeyword()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        GivenHasPermission(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _sender.Setup(x => x.Send(It.IsAny<ApproveChatPlanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var handler = CreateSendPlanChatHandler();
        var result = await handler.Handle(new SendPlanChatMessageCommand(runId, "duyệt", null), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Action.Should().Be("approved");
        _sender.Verify(
            x => x.Send(
                It.Is<ApproveChatPlanCommand>(c => c.RunId == runId && c.Version == plan.Version),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _sidecarStreamClient.Verify(
            x => x.InterpretPlanChatAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<List<PlanStepDto>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "PLAN_CHAT_02 - Gõ 'huỷ' thì gọi RejectChatPlanCommand")]
    public async Task SendPlanChat_RoutesToReject_WhenTextMatchesRejectKeyword()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Drafting, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        GivenHasPermission(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _sender.Setup(x => x.Send(It.IsAny<RejectChatPlanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var handler = CreateSendPlanChatHandler();
        var result = await handler.Handle(new SendPlanChatMessageCommand(runId, "Huỷ đi", null), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Action.Should().Be("rejected");
        _sender.Verify(
            x => x.Send(It.Is<RejectChatPlanCommand>(c => c.RunId == runId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(
        DisplayName = "PLAN_CHAT_03 - Gõ vào ô bình luận riêng của 1 bước: ghép thẳng operation comment, không gọi sidecar")]
    public async Task SendPlanChat_BuildsCommentOperationDirectly_WhenTargetStepIdProvided()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        GivenHasPermission(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _sender.Setup(x => x.Send(It.IsAny<UpdateChatPlanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<ChatPlanDto>.Success(new ChatPlanDto(runId, 2, ChatPlanStatus.Ready, [], "user", null)));
        var handler = CreateSendPlanChatHandler();
        var result = await handler.Handle(
            new SendPlanChatMessageCommand(runId, "Bước này thiếu ngày cụ thể", "s1"),
            CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Action.Should().Be("edited");
        _sidecarStreamClient.Verify(
            x => x.InterpretPlanChatAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<List<PlanStepDto>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _sender.Verify(
            x => x.Send(
                It.Is<UpdateChatPlanCommand>(
                    c => c.Operations.Count == 1 &&
                        c.Operations[0].Type == "comment" &&
                        c.Operations[0].StepId == "s1" &&
                        c.Operations[0].Comment == "Bước này thiếu ngày cụ thể"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(
        DisplayName = "PLAN_CHAT_04 - Free-text chung chung không khớp keyword: gọi sidecar diễn giải rồi UpdateChatPlanCommand")]
    public async Task SendPlanChat_CallsSidecarInterpret_WhenFreeTextDoesNotMatchKeywords()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        GivenHasPermission(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _sidecarStreamClient
            .Setup(
                x => x.InterpretPlanChatAsync(
                    runId,
                    "sửa bước 1 thành lấy doanh thu tháng này",
                    It.IsAny<List<PlanStepDto>>(),
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new PlanChatInterpretationDto(
                    "edit_plan",
                    [new PlanChatInterpretedOperationDto(
                        "edit",
                        "s1",
                        "Lấy doanh thu",
                        "chi tiết mới",
                        null,
                        null,
                        ["get_sales_summary"])],
                    "Đã sửa bước 1."));
        _sender.Setup(x => x.Send(It.IsAny<UpdateChatPlanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChatPlanDto>.Success(new ChatPlanDto(runId, 2, ChatPlanStatus.Ready, [], "ai", null)));
        var handler = CreateSendPlanChatHandler();
        var result = await handler.Handle(
            new SendPlanChatMessageCommand(runId, "sửa bước 1 thành lấy doanh thu tháng này", null),
            CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Action.Should().Be("edited");
        result.Value.Reply.Should().Be("Đã sửa bước 1.");
        _sender.Verify(
            x => x.Send(
                It.Is<UpdateChatPlanCommand>(
                    c => c.Operations.Count == 1 &&
                        c.Operations[0].Type == "edit" &&
                        c.Operations[0].StepId == "s1" &&
                        c.Operations[0].ExpectedTools!.Contains("get_sales_summary")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "PLAN_CHAT_05 - Sidecar trả unclear thì không gọi UpdateChatPlanCommand")]
    public async Task SendPlanChat_ReturnsUnclear_WhenSidecarCannotInterpret()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Ready, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        GivenHasPermission(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _sidecarStreamClient
            .Setup(
                x => x.InterpretPlanChatAsync(
                    runId,
                    "ừm",
                    It.IsAny<List<PlanStepDto>>(),
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlanChatInterpretationDto("unclear", [], "Bạn muốn sửa gì cụ thể vậy?"));
        var handler = CreateSendPlanChatHandler();
        var result = await handler.Handle(new SendPlanChatMessageCommand(runId, "ừm", null), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Action.Should().Be("unclear");
        _sender.Verify(x => x.Send(It.IsAny<UpdateChatPlanCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PLAN_CHAT_06 - Không đủ quyền thì Forbidden, không ghi tin nhắn")]
    public async Task SendPlanChat_ReturnsForbidden_WhenNoPermission()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = CreateSendPlanChatHandler();
        var result = await handler.Handle(new SendPlanChatMessageCommand(runId, "duyệt", null), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Forbidden");
        _chatInsert.Verify(x => x.AddMessage(It.IsAny<ChatMessage>()), Times.Never);
    }

    [Fact(DisplayName = "PLAN_CHAT_07 - Plan đã Executing thì bị từ chối")]
    public async Task SendPlanChat_Rejects_WhenPlanAlreadyExecuting()
    {
        var userId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var plan = GivenPlan(runId, userId, ChatPlanStatus.Executing, [Step("s1", 1)]);
        _currentUser.Setup(x => x.GetUserId()).Returns(userId);
        GivenHasPermission(userId);
        _chatRead.Setup(x => x.GetPlanByRunIdAsync(runId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        var handler = CreateSendPlanChatHandler();
        var result = await handler.Handle(new SendPlanChatMessageCommand(runId, "duyệt", null), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation");
    }
}
