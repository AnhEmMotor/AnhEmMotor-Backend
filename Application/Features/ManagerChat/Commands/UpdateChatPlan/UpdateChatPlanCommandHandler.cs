using Application.ApiContracts.ManagerChat.Requests;
using Application.Common.Models;
using Application.DTOs.Chat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;
using System.Text.Json;

namespace Application.Features.ManagerChat.Commands.UpdateChatPlan;

public class UpdateChatPlanCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatUpdateRepository chatUpdateRepository,
    IChatRunWriter chatRunWriter,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateChatPlanCommand, Result<ChatPlanDto>>
{
    private const int MaxSteps = 8;

    public async Task<Result<ChatPlanDto>> Handle(UpdateChatPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        var plan = await chatReadRepository.GetPlanByRunIdAsync(request.RunId, cancellationToken);
        if (plan == null || plan.Run?.Session?.UserId != userId)
        {
            return Result<ChatPlanDto>.Failure(Error.NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu."));
        }
        if (plan.Status is not (ChatPlanStatus.Drafting or ChatPlanStatus.Ready))
        {
            return Result<ChatPlanDto>.Failure(Error.Validation("Chỉ sửa được plan đang soạn hoặc đang chờ duyệt."));
        }
        if (plan.Version != request.Version)
        {
            return Result<ChatPlanDto>.Failure(Error.Conflict("Kế hoạch vừa được cập nhật, vui lòng tải lại."));
        }
        var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps) ?? [];
        foreach (var op in request.Operations)
        {
            var applyResult = ApplyOperation(steps, op);
            if (applyResult != null)
            {
                return Result<ChatPlanDto>.Failure(applyResult);
            }
        }
        var activeStepCount = steps.Count(s => s.Status != PlanStepStatus.Skipped);
        if (activeStepCount > MaxSteps)
        {
            return Result<ChatPlanDto>.Failure(Error.Validation($"Kế hoạch không được vượt quá {MaxSteps} bước."));
        }
        plan.Steps = JsonSerializer.Serialize(steps);
        plan.Version++;
        plan.LastEditedBy = "user";
        chatUpdateRepository.UpdatePlan(plan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await chatRunWriter.AppendAsync(
            plan.RunId,
            ChatRunEventType.PlanEdited,
            JsonSerializer.Serialize(new { planId = plan.Id, version = plan.Version, editedBy = "user" }));
        return Result<ChatPlanDto>.Success(
            new ChatPlanDto(plan.RunId, plan.Version, plan.Status, steps, plan.LastEditedBy, plan.ApprovedAt));
    }

    private static Error? ApplyOperation(List<PlanStepDto> steps, UpdatePlanStepOperation op)
    {
        switch (op.Type)
        {
            case "edit":
            {
                var idx = steps.FindIndex(s => s.Id == op.StepId);
                if (idx < 0)
                    return Error.Validation($"Không tìm thấy bước '{op.StepId}'.");
                var current = steps[idx];
                if (current.Status is PlanStepStatus.Running or PlanStepStatus.Done)
                {
                    return Error.Validation("Không thể sửa bước đang chạy hoặc đã xong.");
                }
                steps[idx] = current with
                {
                    Title = op.Title ?? current.Title,
                    Detail = op.Detail ?? current.Detail,
                    ExpectedTools = op.ExpectedTools ?? current.ExpectedTools,
                    EditedByUser = true,
                };
                return null;
            }
            case "add":
            {
                var nextOrder = steps.Count == 0 ? 1 : steps.Max(s => s.Order) + 1;
                steps.Add(
                    new PlanStepDto(
                        Guid.NewGuid().ToString("N"),
                        op.Order ?? nextOrder,
                        op.Title ?? string.Empty,
                        op.Detail ?? string.Empty,
                        op.ExpectedTools ?? [],
                        PlanStepStatus.Pending,
                        true,
                        null));
                return null;
            }
            case "remove":
            {
                var idx = steps.FindIndex(s => s.Id == op.StepId);
                if (idx < 0)
                    return Error.Validation($"Không tìm thấy bước '{op.StepId}'.");
                var current = steps[idx];
                if (current.Status is PlanStepStatus.Running or PlanStepStatus.Done)
                {
                    return Error.Validation("Không thể xoá bước đang chạy hoặc đã xong.");
                }
                steps[idx] = current with { Status = PlanStepStatus.Skipped, EditedByUser = true };
                return null;
            }
            case "reorder":
            {
                var idx = steps.FindIndex(s => s.Id == op.StepId);
                if (idx < 0)
                    return Error.Validation($"Không tìm thấy bước '{op.StepId}'.");
                if (op.Order == null)
                    return Error.Validation("Thiếu order khi đổi thứ tự.");
                steps[idx] = steps[idx] with { Order = op.Order.Value, EditedByUser = true };
                return null;
            }
            case "comment":
            {
                var idx = steps.FindIndex(s => s.Id == op.StepId);
                if (idx < 0)
                    return Error.Validation($"Không tìm thấy bước '{op.StepId}'.");
                if (string.IsNullOrWhiteSpace(op.Comment))
                    return Error.Validation("Nội dung bình luận không được để trống.");
                var current = steps[idx];
                if (current.Status is PlanStepStatus.Running or PlanStepStatus.Done)
                {
                    return Error.Validation("Không thể bình luận vào bước đang chạy hoặc đã xong.");
                }
                var comments = new List<PlanStepCommentDto>(current.Comments ?? [])
                {
                    new(Guid.NewGuid().ToString("N"), op.Comment.Trim(), "user", DateTime.UtcNow),
                };
                steps[idx] = current with { Comments = comments, EditedByUser = true };
                return null;
            }
            default:
                return Error.Validation($"Loại thao tác không hợp lệ: '{op.Type}'.");
        }
    }
}
