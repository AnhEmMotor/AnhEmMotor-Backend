using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseRequests.Commands.DeletePurchaseRequest
{
    public sealed class DeletePurchaseRequestCommandHandler(
        IPurchaseRequestReadRepository readRepository,
        IPurchaseRequestDeleteRepository deleteRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeletePurchaseRequestCommand, Result>
    {
        public async Task<Result> Handle(DeletePurchaseRequestCommand request, CancellationToken cancellationToken)
        {
            var pr = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (pr is null)
            {
                return Result.Failure(Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.Id}.", "Id"));
            }

            // Deletion rules:
            // 1. If status is Draft: standard Delete permission is enough (checked at Controller).
            // 2. If status is Sent, Approve, or Reject: requires both Delete and ApproveReject.
            if (!string.Equals(pr.Status, "draft", StringComparison.OrdinalIgnoreCase))
            {
                if (!request.HasApproveRejectPermission)
                {
                    return Result.Failure(Error.BadRequest($"Để xóa yêu cầu mua hàng ở trạng thái '{pr.Status}', bạn cần có thêm quyền phê duyệt (Approve/Reject).", "Status"));
                }
            }

            deleteRepository.Delete(pr);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
    }
}
