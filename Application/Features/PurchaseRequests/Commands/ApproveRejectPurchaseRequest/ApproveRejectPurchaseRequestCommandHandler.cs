using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Services;
using Domain.Constants.PurchaseRequest;
using MediatR;
using System;

namespace Application.Features.PurchaseRequests.Commands.ApproveRejectPurchaseRequest
{
    public class ApproveRejectPurchaseRequestCommandHandler(
        IPurchaseRequestReadRepository readRepository,
        IPurchaseRequestUpdateRepository updateRepository,
        IPurchaseRequestInsertRepository insertRepository,
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
            var currentUserId = currentUserContext.GetUserId();
            var oldStatus = pr.Status;
            pr.Status = request.Status;
            
            if (string.Equals(request.Status, PurchaseRequestStatus.Approve, StringComparison.OrdinalIgnoreCase))
            {
                pr.ApprovedBy = currentUserId;
                pr.RejectedBy = null;
            } else if (string.Equals(request.Status, PurchaseRequestStatus.Reject, StringComparison.OrdinalIgnoreCase))
            {
                pr.RejectedBy = currentUserId;
                pr.ApprovedBy = null;
            }
            updateRepository.Update(pr);

            var auditLog = new Domain.Entities.PurchaseRequestAuditLog
            {
                PurchaseRequest = pr,
                Action = "UpdateStatus",
                ChangedById = currentUserId,
                ChangedAt = DateTimeOffset.UtcNow,
                OldStatusId = oldStatus,
                NewStatusId = pr.Status,
                OldNotes = pr.Note,
                NewNotes = pr.Note
            };
            await insertRepository.InsertAuditLogsAsync([auditLog], cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }
}
