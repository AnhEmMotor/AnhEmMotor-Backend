using Application.Common.Models;
using MediatR;

namespace Application.Features.ManagerChat.Commands.UpdateManagerChatSession;

public record UpdateManagerChatSessionCommand(Guid SessionId, string Title) : IRequest<Result<bool>>;
