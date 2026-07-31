using Application.Common.Models;
using Application.DTOs.Chat;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetChatPlan;

public record GetChatPlanQuery(Guid RunId) : IRequest<Result<ChatPlanDto>>;
