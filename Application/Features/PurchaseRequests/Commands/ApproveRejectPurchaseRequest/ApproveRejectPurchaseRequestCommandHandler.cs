using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;
using System;

namespace Application.Features.PurchaseRequests.Commands.ApproveRejectPurchaseRequest
{
    public sealed class ApproveRejectPurchaseRequestCommandHandler(
        IPurchaseRequestReadRepository readRepository,
        IPurchaseRequestUpdateRepository updateRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext) : IRequestHandler<ApproveRejectPurchaseRequestCommand, Result>
    {
        public async Task<Result> Handle(
            ApproveRejectPurchaseRequestCommand request,
            CancellationToken cancellationToken)
        {
            var pr = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (pr is null)
            {
                return Result.Failure(Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.Id}.", "Id"));
            }
            if (!string.Equals(request.Status, PurchaseRequestStatus.Approve, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(request.Status, PurchaseRequestStatus.Reject, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(Error.BadRequest("Trạng thái phê duyệt không hợp lệ.", "Status"));
            }
            if (!string.Equals(pr.Status, PurchaseRequestStatus.Sent, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(
                    Error.BadRequest(
                        "Chỉ có thể phê duyệt hoặc từ chối yêu cầu mua hàng đang ở trạng thái Sent.",
                        "Status"));
            }
            pr.Status = request.Status;
            pr.ApprovedBy = currentUserContext.GetUserId();
            updateRepository.Update(pr);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }
}
