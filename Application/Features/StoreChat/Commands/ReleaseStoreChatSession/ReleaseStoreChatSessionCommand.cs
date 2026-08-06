using Application.Common.Models;
using MediatR;

namespace Application.Features.StoreChat.Commands.ReleaseStoreChatSession;

public record ReleaseStoreChatSessionCommand(Guid SessionId) : IRequest<Result<bool>>;
