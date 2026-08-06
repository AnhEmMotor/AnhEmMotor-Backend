using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;
using System.Text.Json;

namespace Application.Features.ManagerChat.Commands.RejectChatPlan;

public class RejectChatPlanCommandHandler(
    IChatReadRepository chatReadRepository,
    IChatUpdateRepository chatUpdateRepository,
    IChatRunWriter chatRunWriter,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<RejectChatPlanCommand, Result>
{
    public async Task<Result> Handle(RejectChatPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        var plan = await chatReadRepository.GetPlanByRunIdAsync(request.RunId, cancellationToken);
        if (plan == null || plan.Run?.Session?.UserId != userId)
        {
            return Result.Failure(Error.NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu."));
        }
        if (plan.Status is not (ChatPlanStatus.Drafting or ChatPlanStatus.Ready))
        {
            return Result.Failure(Error.Validation("Không thể huỷ kế hoạch đã duyệt hoặc đã xong."));
        }
        plan.Status = ChatPlanStatus.Rejected;
        chatUpdateRepository.UpdatePlan(plan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await chatRunWriter.AppendAsync(
            plan.RunId,
            ChatRunEventType.PlanRejected,
            JsonSerializer.Serialize(new { planId = plan.Id }));
        await chatRunWriter.CancelAsync(plan.RunId, string.Empty, DateTime.UtcNow);
        return Result.Success();
    }
}
