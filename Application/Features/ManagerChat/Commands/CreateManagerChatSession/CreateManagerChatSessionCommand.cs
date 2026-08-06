using Application.Common.Models;
using MediatR;

namespace Application.Features.ManagerChat.Commands.CreateManagerChatSession;

public record CreateManagerChatSessionCommand(string Title, string InitialMessage = "") : IRequest<Result<CreateManagerChatSessionResponse>>;
