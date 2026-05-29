using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants.InventoryReceipt;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.DeleteInput;

public sealed class DeleteInputCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IPermissionReadRepository permissionRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteInputCommand, Result>
{
    public async Task<Result> Handle(DeleteInputCommand request, CancellationToken cancellationToken)
    {
        var InventoryReceipt = await readRepository.GetByIdAsync(request.Id!.Value, cancellationToken).ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Result.Failure(Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id"));
        }
        if (InputStatus.IsCannotDelete(InventoryReceipt.StatusId))
        {
            return Result.Failure(
                Error.BadRequest($"Không cho phép xóa đơn hàng đã hoàn tất (Approve).", "StatusId"));
        }
        if (!string.Equals(InventoryReceipt.StatusId, InputStatus.Draft, StringComparison.OrdinalIgnoreCase))
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

