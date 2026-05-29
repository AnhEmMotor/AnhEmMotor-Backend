using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.DeleteManyInventoryReceipts;

public sealed class DeleteManyInventoryReceiptsCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptDeleteRepository deleteRepository,
    IPermissionReadRepository permissionRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyInventoryReceiptsCommand, Result>
{
    public async Task<Result> Handle(DeleteManyInventoryReceiptsCommand request, CancellationToken cancellationToken)
    {
        var InventoryReceipts = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);
        var InventoryReceiptsList = InventoryReceipts.ToList();
        if (InventoryReceiptsList.Count != request.Ids.Count)
        {
            var foundIds = InventoryReceiptsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return Result.Failure(
                Error.NotFound($"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}", "Ids"));
        }
        var errors = new List<Error>();
        bool hasApprovePermissionChecked = false;
        bool hasApprovePermission = false;
        Guid userId = currentUserContext.GetUserId();

        foreach (var receipt in InventoryReceiptsList)
        {
            if (InventoryReceiptStatus.IsCannotDelete(receipt.StatusId))
            {
                errors.Add(Error.BadRequest($"Khi đã phê duyệt hoặc từ chối thì không được xoá phiếu (Phiếu ID {receipt.Id}).", "Ids"));
            }
            else if (!string.Equals(receipt.StatusId, InventoryReceiptStatus.Draft, StringComparison.OrdinalIgnoreCase))
            {
                if (!hasApprovePermissionChecked)
                {
                    hasApprovePermission = await permissionRepository.CheckUserPermissionsAsync(
                        userId,
                        [Domain.Constants.Permission.Permissions.InventoryReceipts.ApproveReject],
                        cancellationToken)
                        .ConfigureAwait(false);
                    hasApprovePermissionChecked = true;
                }
                if (!hasApprovePermission)
                {
                    errors.Add(Error.BadRequest($"Để xóa phiếu nhập ở trạng thái '{receipt.StatusId}', bạn cần có quyền phê duyệt/từ chối (Phiếu ID {receipt.Id}).", "Ids"));
                }
            }
        }
        if (errors.Count > 0)
        {
            return Result.Failure(errors);
        }
        deleteRepository.Delete(InventoryReceiptsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
