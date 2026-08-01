using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.StoreChat;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.StoreChat.Commands.LinkStoreChatSessionToCustomer;

public class LinkStoreChatSessionToCustomerCommandHandler(
    IStoreChatReadRepository storeChatReadRepository,
    IStoreChatUpdateRepository storeChatUpdateRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LinkStoreChatSessionToCustomerCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(LinkStoreChatSessionToCustomerCommand request, CancellationToken cancellationToken)
    {
        var session = await storeChatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Error.NotFound("Phiên chat không tồn tại.");
        }

        // Lấy userId từ JWT, không tin dữ liệu client gửi lên — tránh giả mạo gắn nhầm khách khác.
        session.CustomerUserId = currentUserContext.GetUserId();
        storeChatUpdateRepository.UpdateSession(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
