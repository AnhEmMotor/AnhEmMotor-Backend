using Application.Common.Models;
using MediatR;

namespace Application.Features.ManagerChat.Commands.DeleteManagerChatSession;

public record DeleteManagerChatSessionCommand(Guid SessionId) : IRequest<Result<bool>>;
