using Application.Common.Models;
using MediatR;

namespace Application.Features.ManagerChat.Commands.ApproveChatPlan;

public record ApproveChatPlanCommand(Guid RunId, int Version) : IRequest<Result>;
