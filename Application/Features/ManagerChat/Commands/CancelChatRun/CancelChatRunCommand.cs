using MediatR;

namespace Application.Features.ManagerChat.Commands.CancelChatRun;

public record CancelChatRunCommand(Guid RunId, Guid UserId) : IRequest;
