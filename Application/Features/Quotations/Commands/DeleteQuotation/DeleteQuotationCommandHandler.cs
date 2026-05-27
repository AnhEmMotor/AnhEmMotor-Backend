using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Quotation;
using Domain.Constants;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Quotations.Commands.DeleteQuotation
{
    public sealed class DeleteQuotationCommandHandler(
        IQuotationDeleteRepository deleteRepository,
        IQuotationReadRepository readRepository,
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
            if (currentStatus == "approved" && !request.HasApprovePermission)
            {
                return Result.Failure(Error.BadRequest("Báo giá đã xác nhận chỉ có thể xóa bởi người dùng có quyền xác nhận báo giá.", "Status"));
            }

            deleteRepository.Delete(quotation);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
    }
}
