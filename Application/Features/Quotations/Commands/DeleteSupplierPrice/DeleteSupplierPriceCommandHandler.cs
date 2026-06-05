using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Quotation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Quotations.Commands.DeleteSupplierPrice
{
    public sealed class DeleteSupplierPriceCommandHandler(
        IQuotationProductRowRepository quotationRowRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteSupplierPriceCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteSupplierPriceCommand request, CancellationToken cancellationToken)
        {
            var existingQuote = await quotationRowRepository
                .GetBySupplierAndVariantAsync(request.ProductVariantId, request.ProductVariantColorId, request.SupplierId, cancellationToken)
                .ConfigureAwait(false);

            if (existingQuote != null)
            {
                quotationRowRepository.Delete(existingQuote);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return Result<bool>.Success(true);
            }

            return Error.NotFound("Không tìm thấy giá nhà cung cấp cần xóa.", "SupplierId");
        }
    }
}
