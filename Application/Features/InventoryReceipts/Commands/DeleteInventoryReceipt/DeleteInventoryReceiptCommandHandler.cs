using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants.InventoryReceipt;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.DeleteInventoryReceipt;

public class DeleteInventoryReceiptCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptDeleteRepository deleteRepository,
    IPermissionReadRepository permissionRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteInventoryReceiptCommand, Result>
{
    public async Task<Result> Handle(DeleteInventoryReceiptCommand request, CancellationToken cancellationToken)
    {
        var InventoryReceipt = await readRepository.GetByIdAsync(request.Id!.Value, cancellationToken)
            .ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Result.Failure(Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id"));
        }
        if (InventoryReceiptStatus.IsCannotDelete(InventoryReceipt.StatusId))
        {
            return Result.Failure(
                Error.BadRequest("Khi đã phê duyệt hoặc từ chối thì không được xoá phiếu.", "StatusId"));
        }
        if (!string.Equals(InventoryReceipt.StatusId, InventoryReceiptStatus.Draft, StringComparison.OrdinalIgnoreCase))
        {
            Guid userId = currentUserContext.GetUserId();
            var hasApprovePermission = await permissionRepository.CheckUserPermissionsAsync(
                userId,
                [Domain.Constants.Permission.Permissions.Warehouse.ReceiptManagement.ApproveReject],
                cancellationToken)
                .ConfigureAwait(false);
            if (!hasApprovePermission)
            {
                return Result.Failure(
                    Error.BadRequest(
                        $"Để xóa phiếu nhập ở trạng thái '{InventoryReceipt.StatusId}', bạn cần có quyền phê duyệt/từ chối (Approve/Reject).",
                        "StatusId"));
            }
        }
        deleteRepository.Delete(InventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

