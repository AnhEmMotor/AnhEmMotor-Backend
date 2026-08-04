using Application.Common.Models;
using Application.DTOs.Chat;
using MediatR;

namespace Application.Features.ManagerChat.Commands.SendPlanChatMessage;

public record SendPlanChatMessageCommand(Guid RunId, string Content, string? TargetStepId) : IRequest<Result<PlanChatResultDto>>;
