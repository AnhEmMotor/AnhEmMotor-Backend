using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Commands.DeleteStoreChatSession;

public class DeleteStoreChatSessionCommandHandler(
    IStoreChatReadRepository storeChatReadRepository,
    IStoreChatDeleteRepository storeChatDeleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteStoreChatSessionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteStoreChatSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await storeChatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Error.NotFound("Phiên chat không tồn tại.");
        }
        await storeChatDeleteRepository.DeleteSessionAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
