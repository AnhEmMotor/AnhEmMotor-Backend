using Application.Common.Models;
using Application.Interfaces.Repositories.StoreChat;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.StoreChat.Commands.ReleaseStoreChatSession;

public class ReleaseStoreChatSessionCommandHandler(
    IStoreChatUpdateRepository storeChatUpdateRepository,
    ILogger<ReleaseStoreChatSessionCommandHandler> logger)
    : IRequestHandler<ReleaseStoreChatSessionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ReleaseStoreChatSessionCommand request, CancellationToken cancellationToken)
    {
        var released = await storeChatUpdateRepository.TryReleaseAsync(request.SessionId, cancellationToken);
        if (!released)
        {
            return Error.Conflict("Phiên không ở trạng thái đang có nhân viên xử lý.");
        }

        logger.LogInformation("[StoreChat] Action=Release SessionId={SessionId}", request.SessionId);
        return true;
    }
}
