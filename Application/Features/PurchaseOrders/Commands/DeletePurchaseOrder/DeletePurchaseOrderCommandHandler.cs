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

namespace Application.Features.PurchaseOrders.Commands.DeletePurchaseOrder
{
    public sealed class DeletePurchaseOrderCommandHandler(
        IPurchaseOrderReadRepository readRepository,
        IPurchaseOrderDeleteRepository deleteRepository,
        IPermissionReadRepository permissionReadRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<DeletePurchaseOrderCommand, Result>
    {
        public async Task<Result> Handle(DeletePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var po = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (po is null)
            {
                return Result.Failure(Error.NotFound($"Không tìm thấy đơn chốt mua có ID {request.Id}.", "Id"));
            }

            var userId = currentUserContext.GetUserId();

            if (string.Equals(po.Status, PurchaseOrderStatus.Draft, StringComparison.OrdinalIgnoreCase))
            {
                if (!await permissionReadRepository.CheckUserPermissionsAsync(
                    userId,
                    [Domain.Constants.Permission.Permissions.InventoryReceipts.Delete],
                    cancellationToken)
                    .ConfigureAwait(false))
                {
                    return Result.Failure(Error.Forbidden("Bạn không có quyền xóa đơn chốt mua.", "Permission"));
                }
            }
            else if (string.Equals(po.Status, PurchaseOrderStatus.Sent, StringComparison.OrdinalIgnoreCase))
            {
                var hasDelete = await permissionReadRepository.CheckUserPermissionsAsync(userId, [Domain.Constants.Permission.Permissions.InventoryReceipts.Delete], cancellationToken).ConfigureAwait(false);
                var hasApproveReject = await permissionReadRepository.CheckUserPermissionsAsync(userId, [Domain.Constants.Permission.Permissions.InventoryReceipts.ApproveReject], cancellationToken).ConfigureAwait(false);
                if (!hasDelete || !hasApproveReject)
                {
                    return Result.Failure(Error.Forbidden("Bạn cần có cả quyền Delete và Approve/Reject để xóa đơn chốt mua ở trạng thái Sent.", "Permission"));
                }
            }
            else if (string.Equals(po.Status, PurchaseOrderStatus.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                if (!await permissionReadRepository.CheckUserPermissionsAsync(
                    userId,
                    [Domain.Constants.Permission.Permissions.InventoryReceipts.Delete],
                    cancellationToken)
                    .ConfigureAwait(false))
                {
                    return Result.Failure(Error.Forbidden("Bạn không có quyền xóa đơn chốt mua.", "Permission"));
                }
            }
            else if (string.Equals(po.Status, PurchaseOrderStatus.Approved, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(Error.BadRequest("Không thể xóa đơn chốt mua đã được Approved.", "Status"));
            }
            else
            {
                return Result.Failure(Error.BadRequest("Không thể xóa đơn chốt mua ở trạng thái hiện tại.", "Status"));
            }

            deleteRepository.Delete(po);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
    }
}
