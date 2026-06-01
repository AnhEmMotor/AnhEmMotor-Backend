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

namespace Application.Features.PurchaseOrders.Commands.SendPurchaseOrder
{
    public sealed class SendPurchaseOrderCommandHandler(
        IPurchaseOrderReadRepository readRepository,
        IPurchaseOrderUpdateRepository updateRepository,
        IPermissionReadRepository permissionReadRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<SendPurchaseOrderCommand, Result>
    {
        public async Task<Result> Handle(SendPurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var po = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (po is null)
            {
                return Result.Failure(Error.NotFound($"Không tìm thấy đơn chốt mua có ID {request.Id}.", "Id"));
            }

            var userId = currentUserContext.GetUserId();

            // Check Send permission
            if (!await permissionReadRepository.CheckUserPermissionsAsync(
                userId,
                [Domain.Constants.Permission.Permissions.InventoryReceipts.Send],
                cancellationToken)
                .ConfigureAwait(false))
            {
                return Result.Failure(Error.Forbidden("Bạn không có quyền gửi đơn chốt mua.", "Permission"));
            }

            if (!string.Equals(po.Status, PurchaseOrderStatus.Draft, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(
                    Error.BadRequest(
                        "Chỉ có thể gửi đơn chốt mua khi ở trạng thái Draft.",
                        "Status"));
            }

            po.Status = PurchaseOrderStatus.Sent;
            po.SentBy = userId;

            updateRepository.Update(po);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
    }
}
