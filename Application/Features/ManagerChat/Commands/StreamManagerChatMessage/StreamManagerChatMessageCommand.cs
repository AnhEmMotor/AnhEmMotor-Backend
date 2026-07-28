using MediatR;

namespace Application.Features.ManagerChat.Commands.StreamManagerChatMessage;

public record StreamManagerChatMessageCommand(Guid SessionId, string Content, Guid UserId, string Token) : IStreamRequest<string>;
