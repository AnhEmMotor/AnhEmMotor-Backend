using Application.ApiContracts.ManagerChat.Requests;
using Application.Common.Models;
using Application.DTOs.Chat;
using MediatR;

namespace Application.Features.ManagerChat.Commands.UpdateChatPlan;

public record UpdateChatPlanCommand(Guid RunId, int Version, List<UpdatePlanStepOperation> Operations)
    : IRequest<Result<ChatPlanDto>>;
