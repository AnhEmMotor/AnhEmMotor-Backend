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

            deleteRepository.Delete(quotation);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
    }
}
