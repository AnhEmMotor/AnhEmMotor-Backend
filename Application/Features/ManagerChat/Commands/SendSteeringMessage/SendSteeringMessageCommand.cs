using Application.Common.Models;
using Application.DTOs.Chat;
using MediatR;

namespace Application.Features.ManagerChat.Commands.SendSteeringMessage;

public record SendSteeringMessageCommand(Guid RunId, string Content, Guid UserId, string Token)
    : IRequest<Result<SteeringResultDto>>;
