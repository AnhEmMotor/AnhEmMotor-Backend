using System.Text.Json;
using Application.ApiContracts.ManagerChat.Requests;
using Application.Common.Models;
using Application.DTOs.Chat;
using Application.Features.ManagerChat.Commands.ApproveChatPlan;
using Application.Features.ManagerChat.Commands.RejectChatPlan;
using Application.Features.ManagerChat.Commands.UpdateChatPlan;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.ManagerChat.Commands.SendPlanChatMessage;

/// <summary>Thay cho nút Duyệt/Huỷ trên PlanCard (Stage 10.9): mọi tin nhắn gõ trong lúc plan
/// Drafting/Ready đi qua đây thay vì SendSteering (vốn cố ý từ chối AwaitingApproval vì graph đã
/// kết thúc — route plan→END, không interrupt). Chỉ điều phối, KHÔNG viết lại nghiệp vụ đã có ở
/// Approve/Reject/UpdateChatPlan — mọi ownership/version-conflict/permission vẫn do 3 handler đó
/// tự kiểm tra khi được gọi lại qua ISender.</summary>
public class SendPlanChatMessageCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatInsertRepository chatInsertRepository,
    IPermissionReadRepository permissionReadRepository,
    ISidecarStreamClient sidecarStreamClient,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork,
    ISender sender) : IRequestHandler<SendPlanChatMessageCommand, Result<PlanChatResultDto>>
{
    public async Task<Result<PlanChatResultDto>> Handle(SendPlanChatMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();

        var hasPermission = await permissionReadRepository.HasAnyPermissionAsync(userId, cancellationToken);
        if (!hasPermission)
        {
            return Result<PlanChatResultDto>.Failure(Error.Forbidden());
        }

        var plan = await chatReadRepository.GetPlanByRunIdAsync(request.RunId, cancellationToken);
        if (plan == null || plan.Run?.Session?.UserId != userId)
        {
            return Result<PlanChatResultDto>.Failure(Error.NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu."));
        }

        if (plan.Status is not (ChatPlanStatus.Drafting or ChatPlanStatus.Ready))
        {
            return Result<PlanChatResultDto>.Failure(
                Error.Validation("Chỉ chat để sửa/duyệt/huỷ được khi kế hoạch đang soạn hoặc đang chờ duyệt."));
        }

        chatInsertRepository.AddMessage(new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = plan.SessionId,
            Role = ChatRole.User,
            Message = request.Content,
            CreatedAt = DateTime.UtcNow,
        });
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.TargetStepId == null)
        {
            var action = PlanChatClassifier.Classify(request.Content);
            if (action == "approve")
            {
                var approveResult = await sender.Send(new ApproveChatPlanCommand(request.RunId, plan.Version), cancellationToken);
                return approveResult.IsFailure
                    ? Result<PlanChatResultDto>.Failure(approveResult.Error!)
                    : Result<PlanChatResultDto>.Success(new PlanChatResultDto("approved", null, "Đã duyệt kế hoạch."));
            }
            if (action == "reject")
            {
                var rejectResult = await sender.Send(new RejectChatPlanCommand(request.RunId), cancellationToken);
                return rejectResult.IsFailure
                    ? Result<PlanChatResultDto>.Failure(rejectResult.Error!)
                    : Result<PlanChatResultDto>.Success(new PlanChatResultDto("rejected", null, "Đã huỷ kế hoạch."));
            }
        }

        List<UpdatePlanStepOperation> operations;
        string reply;
        if (request.TargetStepId != null)
        {
            // Gõ vào đúng ô bình luận của 1 bước — đã rõ ràng, không cần LLM diễn giải.
            operations = [new UpdatePlanStepOperation
            {
                Type = "comment", StepId = request.TargetStepId, Comment = request.Content,
            }];
            reply = "Đã ghi nhận bình luận của bạn.";
        }
        else
        {
            var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps) ?? [];
            var interpretation = await sidecarStreamClient.InterpretPlanChatAsync(
                request.RunId, request.Content, steps, null, cancellationToken);

            if (interpretation.Intent == "unclear" || interpretation.Operations.Count == 0)
            {
                return Result<PlanChatResultDto>.Success(new PlanChatResultDto("unclear", null, interpretation.Reply));
            }

            operations = interpretation.Operations.Select(o => new UpdatePlanStepOperation
            {
                Type = o.Type,
                StepId = o.StepId,
                Title = o.Title,
                Detail = o.Detail,
                Comment = o.Comment,
                Order = o.Order,
                ExpectedTools = o.ExpectedTools,
            }).ToList();
            reply = interpretation.Reply;
        }

        var updateResult = await sender.Send(new UpdateChatPlanCommand(request.RunId, plan.Version, operations), cancellationToken);
        return updateResult.IsFailure
            ? Result<PlanChatResultDto>.Failure(updateResult.Error!)
            : Result<PlanChatResultDto>.Success(new PlanChatResultDto("edited", updateResult.Value, reply));
    }
}
