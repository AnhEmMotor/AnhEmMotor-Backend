using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants.Input;
using Domain.Constants.Permission.Permissions;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed class DeleteInputCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IPermissionReadRepository permissionRepository,
    IHttpTokenAccessorService httpTokenAccessorService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteInputCommand, Result>
{
    public async Task<Result> Handle(DeleteInputCommand request, CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(request.Id!.Value, cancellationToken).ConfigureAwait(false);
        if (input is null)
        {
            return Result.Failure(Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id"));
        }
        if (InputStatus.IsCannotDelete(input.StatusId))
        {
            return Result.Failure(
                Error.BadRequest($"Không cho phép xóa đơn hàng đã hoàn tất (Approve).", "StatusId"));
        }
        if (!string.Equals(input.StatusId, InputStatus.Draft, StringComparison.OrdinalIgnoreCase))
        {
            var userIdStr = httpTokenAccessorService.GetUserId();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Result.Failure(Error.Forbidden("Không xác định được danh tính người dùng."));
            }

            var hasApprovePermission = await permissionRepository.CheckUserPermissionsAsync(
                userId,
                [PurchaseOrders.ApproveReject],
                cancellationToken)
                .ConfigureAwait(false);

            if (!hasApprovePermission)
            {
                return Result.Failure(
                   Error.BadRequest($"Để xóa đơn hàng ở trạng thái '{input.StatusId}', bạn cần có thêm quyền phê duyệt (Approve/Reject).", "StatusId"));
            }
        }
        deleteRepository.Delete(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

