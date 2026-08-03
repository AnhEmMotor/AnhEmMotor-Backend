using Application.Common.Models;
using MediatR;

namespace Application.Features.StoreChat.Commands.DeleteStoreChatSession;

public record DeleteStoreChatSessionCommand(Guid SessionId) : IRequest<Result<bool>>;
