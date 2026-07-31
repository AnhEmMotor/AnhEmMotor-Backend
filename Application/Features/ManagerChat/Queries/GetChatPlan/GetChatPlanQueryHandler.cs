using System.Text.Json;
using Application.Common.Models;
using Application.DTOs.Chat;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetChatPlan;

public class GetChatPlanQueryHandler(
    IChatReadRepository chatReadRepository,
    ICurrentUserContext currentUserContext) : IRequestHandler<GetChatPlanQuery, Result<ChatPlanDto>>
{
    public async Task<Result<ChatPlanDto>> Handle(GetChatPlanQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();

        var plan = await chatReadRepository.GetPlanByRunIdAsync(request.RunId, cancellationToken);
        if (plan == null || plan.Run?.Session?.UserId != userId)
        {
            return Result<ChatPlanDto>.Failure(Error.NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu."));
        }

        var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps) ?? [];
        return Result<ChatPlanDto>.Success(
            new ChatPlanDto(plan.RunId, plan.Version, plan.Status, steps, plan.LastEditedBy, plan.ApprovedAt));
    }
}
