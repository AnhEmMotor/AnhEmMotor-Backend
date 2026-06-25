using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductQuotations;
using Domain.Entities;
using MediatR;

namespace Application.Features.ProductQuotations.Commands.SaveSupplierPrice
{
    public class SaveSupplierPriceCommandHandler(
        IProductQuotationReadRepository quotationReadRepository,
        IProductQuotationUpdateRepository quotationUpdateRepository,
        IProductQuotationInsertRepository quotationInsertRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<SaveSupplierPriceCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(SaveSupplierPriceCommand request, CancellationToken cancellationToken)
        {
            var existingQuote = await quotationReadRepository.GetBySupplierAndVariantAsync(
                request.ProductVariantId,
                request.ProductVariantColorId,
                request.SupplierId,
                cancellationToken)
                .ConfigureAwait(false);
            if (existingQuote != null)
            {
                existingQuote.QuotePrice = request.QuotePrice;
                existingQuote.Note = request.Note;
                quotationUpdateRepository.Update(existingQuote);
            } else
            {
                var newQuote = new ProductQuotation
                {
                    ProductVariantId = request.ProductVariantId,
                    ProductVariantColorId = request.ProductVariantColorId,
                    SupplierId = request.SupplierId,
                    QuotePrice = request.QuotePrice,
                    Note = request.Note
                };
                await quotationInsertRepository.AddAsync(newQuote, cancellationToken).ConfigureAwait(false);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<bool>.Success(true);
        }
    }
}

