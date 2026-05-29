using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.DeleteInventoryReceipt;

public sealed class DeleteInventoryReceiptCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptDeleteRepository deleteRepository,
    IPermissionReadRepository permissionRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteInventoryReceiptCommand, Result>
{
    public async Task<Result> Handle(DeleteInventoryReceiptCommand request, CancellationToken cancellationToken)
    {
        var InventoryReceipt = await readRepository.GetByIdAsync(request.Id!.Value, cancellationToken).ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Result.Failure(Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id"));
        }
        if (InventoryReceiptStatus.IsCannotDelete(InventoryReceipt.StatusId))
        {
            return Result.Failure(
                Error.BadRequest($"Không cho phép xóa đơn hàng đã hoàn tất (Approve).", "StatusId"));
        }
        if (!string.Equals(InventoryReceipt.StatusId, InventoryReceiptStatus.Draft, StringComparison.OrdinalIgnoreCase))
        {
            Guid userId = currentUserContext.GetUserId();

            var hasApprovePermission = await permissionRepository.CheckUserPermissionsAsync(
                userId,
                [Domain.Constants.Permission.Permissions.InventoryReceipts.ApproveReject],
                cancellationToken)
                .ConfigureAwait(false);

            if (!hasApprovePermission)
            {
                return Result.Failure(
                   Error.BadRequest($"Để xóa đơn hàng ở trạng thái '{InventoryReceipt.StatusId}', bạn cần có thêm quyền phê duyệt (Approve/Reject).", "StatusId"));
            }
        }
        deleteRepository.Delete(InventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

