using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using System;

namespace Application.Features.PurchaseRequests.Commands.DeletePurchaseRequest
{
    public class DeletePurchaseRequestCommandHandler(
        IPurchaseRequestReadRepository readRepository,
        IPurchaseRequestDeleteRepository deleteRepository,
        IPurchaseRequestInsertRepository insertRepository,
        IPermissionReadRepository permissionReadRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<DeletePurchaseRequestCommand, Result>
    {
        public async Task<Result> Handle(DeletePurchaseRequestCommand request, CancellationToken cancellationToken)
        {
            var pr = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (pr is null)
            {
                return Result.Failure(Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.Id}.", "Id"));
            }
            if (!string.Equals(pr.Status, "draft", StringComparison.OrdinalIgnoreCase))
            {
                var userId = currentUserContext.GetUserId();
                if (!await permissionReadRepository.CheckUserPermissionsAsync(
                    userId,
                    [Domain.Constants.Permission.Permissions.Warehouse.PurchaseRequestManagement.ApproveReject],
                    cancellationToken)
                    .ConfigureAwait(false))
                {
                    return Result.Failure(
                        Error.BadRequest(
                            $"Để xóa yêu cầu mua hàng ở trạng thái '{pr.Status}', bạn cần có thêm quyền phê duyệt (Approve/Reject).",
                            "Status"));
                }
            }
            var currentUserId = currentUserContext.GetUserId();
            var auditLog = new PurchaseRequestAuditLog
            {
                PurchaseRequest = pr,
                Action = "Delete",
                ChangedById = currentUserId,
                ChangedAt = DateTimeOffset.UtcNow,
                OldStatusId = pr.Status,
                NewStatusId = "deleted",
                OldNotes = pr.Note,
                NewNotes = pr.Note
            };
            await insertRepository.InsertAuditLogsAsync([auditLog], cancellationToken).ConfigureAwait(false);
            deleteRepository.Delete(pr);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }
}
