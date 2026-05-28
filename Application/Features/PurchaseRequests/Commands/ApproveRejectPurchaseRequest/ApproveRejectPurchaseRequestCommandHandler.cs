using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseRequests.Commands.ApproveRejectPurchaseRequest
{
    public sealed class ApproveRejectPurchaseRequestCommandHandler(
        IPurchaseRequestReadRepository readRepository,
        IPurchaseRequestUpdateRepository updateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<ApproveRejectPurchaseRequestCommand, Result>
    {
        public async Task<Result> Handle(ApproveRejectPurchaseRequestCommand request, CancellationToken cancellationToken)
        {
            var pr = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (pr is null)
            {
                return Result.Failure(Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.Id}.", "Id"));
            }

            if (!string.Equals(pr.Status, "sent", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(Error.BadRequest("Chỉ có thể phê duyệt hoặc từ chối yêu cầu mua hàng đang ở trạng thái Sent.", "Status"));
            }

            pr.Status = request.Approve ? "approve" : "reject";
            pr.ApprovedBy = request.CurrentUserId;
            
            updateRepository.Update(pr);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
    }
}
