using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.Quotation;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Quotations.Commands.DeleteQuotation
{
    public sealed class DeleteQuotationCommandHandler(
        IQuotationDeleteRepository deleteRepository,
        IQuotationReadRepository readRepository,
        IPermissionReadRepository permissionRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteQuotationCommand, Result>
    {
        public async Task<Result> Handle(DeleteQuotationCommand request, CancellationToken cancellationToken)
        {
            var quotation = await readRepository.GetByIdAsync(
                request.Id!.Value,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            if (quotation is null)
            {
                return Result.Failure(Error.NotFound($"Yêu cầu báo giá {request.Id} không tồn tại hoặc đã bị xóa.", "Id"));
            }

            var currentStatus = quotation.Status?.ToLower();
            if (currentStatus == "approved" || currentStatus == "sent")
            {
                Guid userId = currentUserContext.GetUserId();
                var hasApprovePermission = await permissionRepository.CheckUserPermissionsAsync(
                    userId,
                    [Domain.Constants.Permission.Permissions.Quotations.Approve],
                    cancellationToken)
                    .ConfigureAwait(false);

                if (!hasApprovePermission)
                {
                    return Result.Failure(Error.BadRequest("Báo giá ở trạng thái đã gửi hoặc đã duyệt chỉ có thể xóa bởi người dùng có quyền duyệt/hủy báo giá.", "Status"));
                }
            }

            deleteRepository.Delete(quotation);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
    }
}
