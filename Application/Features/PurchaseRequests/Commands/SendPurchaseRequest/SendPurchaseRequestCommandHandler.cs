using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Services;
using MediatR;
using System;

namespace Application.Features.PurchaseRequests.Commands.SendPurchaseRequest
{
    public class SendPurchaseRequestCommandHandler(
        IPurchaseRequestReadRepository readRepository,
        IPurchaseRequestUpdateRepository updateRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserContext? currentUserContext = null) : IRequestHandler<SendPurchaseRequestCommand, Result>
    {
        public async Task<Result> Handle(SendPurchaseRequestCommand request, CancellationToken cancellationToken)
        {
            var pr = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (pr is null)
            {
                return Result.Failure(Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.Id}.", "Id"));
            }
            if (!string.Equals(pr.Status, "draft", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(
                    Error.BadRequest("Chỉ có thể gửi yêu cầu mua hàng đang ở trạng thái Nháp (Draft).", "Status"));
            }
            pr.Status = "sent";
            pr.SentBy = currentUserContext?.GetUserId();
            updateRepository.Update(pr);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }
}
