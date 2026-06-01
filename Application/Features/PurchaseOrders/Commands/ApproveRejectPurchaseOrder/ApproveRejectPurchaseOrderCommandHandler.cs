using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.PurchaseOrder;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Permission.Permissions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseOrders.Commands.ApproveRejectPurchaseOrder
{
    public sealed class ApproveRejectPurchaseOrderCommandHandler(
        IPurchaseOrderReadRepository readRepository,
        IPurchaseOrderUpdateRepository updateRepository,
        IPermissionReadRepository permissionReadRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<ApproveRejectPurchaseOrderCommand, Result>
    {
        public async Task<Result> Handle(
            ApproveRejectPurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            var po = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (po is null)
            {
                return Result.Failure(Error.NotFound($"Không tìm thấy đơn chốt mua có ID {request.Id}.", "Id"));
            }

            var userId = currentUserContext.GetUserId();

            // Check ApproveReject permission
            if (!await permissionReadRepository.CheckUserPermissionsAsync(
                userId,
                [Domain.Constants.Permission.Permissions.InventoryReceipts.ApproveReject],
                cancellationToken)
                .ConfigureAwait(false))
            {
                return Result.Failure(Error.Forbidden("Bạn không có quyền phê duyệt hoặc từ chối đơn chốt mua.", "Permission"));
            }

            if (!string.Equals(request.Status, PurchaseOrderStatus.Approved, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(request.Status, PurchaseOrderStatus.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(Error.BadRequest("Trạng thái phê duyệt không hợp lệ.", "Status"));
            }

            if (!string.Equals(po.Status, PurchaseOrderStatus.Sent, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(
                    Error.BadRequest(
                        "Chỉ có thể phê duyệt hoặc từ chối đơn chốt mua đang ở trạng thái Sent.",
                        "Status"));
            }

            po.Status = request.Status;
            if (string.Equals(request.Status, PurchaseOrderStatus.Approved, StringComparison.OrdinalIgnoreCase))
            {
                po.ApprovedBy = userId;
                po.RejectedBy = null;
            }
            else if (string.Equals(request.Status, PurchaseOrderStatus.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                po.RejectedBy = userId;
                po.ApprovedBy = null;
            }

            updateRepository.Update(po);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
    }
}
