using Application.Common.Models;
using MediatR;

namespace Application.Features.ManagerChat.Commands.RejectChatPlan;

public record RejectChatPlanCommand(Guid RunId) : IRequest<Result>;
