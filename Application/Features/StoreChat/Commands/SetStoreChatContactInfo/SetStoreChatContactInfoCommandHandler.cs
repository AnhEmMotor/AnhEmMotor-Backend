using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Commands.SetStoreChatContactInfo;

public class SetStoreChatContactInfoCommandHandler(
    IStoreChatReadRepository storeChatReadRepository,
    IStoreChatUpdateRepository storeChatUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SetStoreChatContactInfoCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(SetStoreChatContactInfoCommand request, CancellationToken cancellationToken)
    {
        var session = await storeChatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Error.NotFound("Phiên chat không tồn tại.");
        }
        session.ContactName = request.ContactName;
        session.ContactPhone = request.ContactPhone;
        storeChatUpdateRepository.UpdateSession(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
