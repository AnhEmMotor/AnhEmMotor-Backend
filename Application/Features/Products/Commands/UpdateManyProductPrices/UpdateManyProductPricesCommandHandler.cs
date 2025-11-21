using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed class UpdateManyProductPricesCommandHandler(
    IProductSelectRepository selectRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateManyProductPricesCommand, (List<int>? Data, ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, ErrorResponse? Error)> Handle(UpdateManyProductPricesCommand command, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var productIds = command.Ids;
        var newPrice = command.Price;

        var allProducts = await selectRepository.GetActiveProducts()
            .Where(p => productIds.Contains(p.Id))
            .Include(p => p.ProductVariants)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (allProducts.Count != productIds.Count)
        {
            var foundIds = allProducts.Select(p => p.Id).ToHashSet();
            var missingIds = productIds.Where(id => !foundIds.Contains(id)).ToList();
            
            foreach (var missingId in missingIds)
            {
                errors.Add(new ErrorDetail
                {
                    Field = missingId.ToString(),
                    Message = $"Sản phẩm với Id {missingId} không tồn tại."
                });
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        foreach (var product in allProducts)
        {
            if (product.ProductVariants != null)
            {
                foreach (var variant in product.ProductVariants)
                {
                    variant.Price = newPrice;
                }
            }

            updateRepository.Update(product);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ([.. allProducts.Select(p => p.Id)], null);
    }
}
