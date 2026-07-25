using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.ManagerChat.Commands.SendManagerChatMessage;

public record SendManagerChatMessageCommand(Guid SessionId, string Content) : IRequest<Result<ChatMessage>>;
