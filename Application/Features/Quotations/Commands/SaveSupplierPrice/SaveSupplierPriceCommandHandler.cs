using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Quotation;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Quotations.Commands.SaveSupplierPrice
{
    public sealed class SaveSupplierPriceCommandHandler(
        IQuotationProductRowRepository quotationRowRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<SaveSupplierPriceCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(SaveSupplierPriceCommand request, CancellationToken cancellationToken)
        {
            var existingQuote = await quotationRowRepository
                .GetBySupplierAndVariantAsync(request.ProductVariantId, request.ProductVariantColorId, request.SupplierId, cancellationToken)
                .ConfigureAwait(false);

            if (existingQuote != null)
            {
                existingQuote.QuotePrice = request.QuotePrice;
                existingQuote.Note = request.Note;
                quotationRowRepository.Update(existingQuote);
            }
            else
            {
                var newQuote = new QuotationProductRow
                {
                    ProductVariantId = request.ProductVariantId,
                    ProductVariantColorId = request.ProductVariantColorId,
                    SupplierId = request.SupplierId,
                    QuotePrice = request.QuotePrice,
                    Note = request.Note
                };
                await quotationRowRepository.AddAsync(newQuote, cancellationToken).ConfigureAwait(false);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<bool>.Success(true);
        }
    }
}
