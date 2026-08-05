using Application.Common.Models;
using Application.DTOs.Chat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;
using System.Text.Json;

namespace Application.Features.ManagerChat.Commands.ApproveChatPlan;

public class ApproveChatPlanCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatUpdateRepository chatUpdateRepository,
    IPermissionReadRepository permissionReadRepository,
    IChatRunWriter chatRunWriter,
    IChatRunTokenStore tokenStore,
    IChatRunQueue chatRunQueue,
    ISidecarStreamClient sidecarStreamClient,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<ApproveChatPlanCommand, Result>
{
    public async Task<Result> Handle(ApproveChatPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        var plan = await chatReadRepository.GetPlanByRunIdAsync(request.RunId, cancellationToken);
        if (plan == null || plan.Run?.Session?.UserId != userId)
        {
            return Result.Failure(Error.NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu."));
        }
        if (plan.Status != ChatPlanStatus.Ready)
        {
            return Result.Failure(Error.Validation("Chỉ duyệt được khi kế hoạch đã sẵn sàng."));
        }
        if (plan.Version != request.Version)
        {
            return Result.Failure(Error.Conflict("Kế hoạch vừa được cập nhật, vui lòng tải lại."));
        }
        var hasPermission = await permissionReadRepository.HasAnyPermissionAsync(userId, cancellationToken);
        if (!hasPermission)
        {
            return Result.Failure(Error.Forbidden());
        }
        var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps) ?? [];
        var expectedTools = steps
            .Where(s => s.Status == PlanStepStatus.Pending)
            .SelectMany(s => s.ExpectedTools)
            .Distinct()
            .ToList();
        var revalidation = await sidecarStreamClient.RevalidatePlanAsync(
            plan.RunId,
            expectedTools,
            plan.ToolRegistryFingerprint,
            cancellationToken);
        if (!revalidation.Ok)
        {
            var unavailable = new HashSet<string>(revalidation.UnavailableTools);
            var invalidatedSteps = steps
                .Select(
                    s => s.Status == PlanStepStatus.Pending && s.ExpectedTools.Any(unavailable.Contains)
                        ? s with { Status = PlanStepStatus.Invalid }
                        : s)
                .ToList();
            plan.Steps = JsonSerializer.Serialize(invalidatedSteps);
            plan.Status = ChatPlanStatus.Drafting;
            plan.Version++;
            plan.LastEditedBy = "ai";
            chatUpdateRepository.UpdatePlan(plan);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await chatRunWriter.AppendAsync(
                plan.RunId,
                ChatRunEventType.PlanInvalidated,
                JsonSerializer.Serialize(
                    new { planId = plan.Id, version = plan.Version, unavailableTools = revalidation.UnavailableTools }));
            return Result.Failure(
                Error.Validation(
                    "Hệ thống đã cập nhật, một số bước trong kế hoạch không còn khả dụng. Vui lòng xem lại kế hoạch."));
        }
        tokenStore.Store(plan.RunId, currentUserContext.GetAccessToken());
        plan.Status = ChatPlanStatus.Executing;
        plan.ApprovedAt = DateTime.UtcNow;
        plan.Version++;
        plan.LastEditedBy = "user";
        chatUpdateRepository.UpdatePlan(plan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await chatRunWriter.AppendAsync(
            plan.RunId,
            ChatRunEventType.PlanApproved,
            JsonSerializer.Serialize(new { planId = plan.Id, version = plan.Version }));
        await chatRunQueue.EnqueueAsync(plan.RunId, cancellationToken);
        return Result.Success();
    }
}
