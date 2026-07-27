using MediatR;

namespace Application.Features.ManagerChat.Commands.StartChatRun;

public record StartChatRunCommand(Guid SessionId, string Content, Guid UserId, string Token) : IRequest<Guid>;
