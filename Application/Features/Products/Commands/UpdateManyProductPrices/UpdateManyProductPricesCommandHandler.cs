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
        var productNames = command.ProductPrices.Keys.ToList();

        var allProducts = await selectRepository.GetActiveProducts()
            .Where(p => p.Name != null && productNames.Contains(p.Name))
            .Include(p => p.ProductVariants)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var productMap = allProducts.ToDictionary(p => p.Name!, StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in command.ProductPrices)
        {
            var productName = kvp.Key;

            if (!productMap.TryGetValue(productName, out var product))
            {
                errors.Add(new ErrorDetail
                {
                    Field = productName,
                    Message = $"Sản phẩm '{productName}' không tồn tại."
                });
                continue;
            }

            var newPrice = kvp.Value;
            if (product.ProductVariants != null)
            {
                foreach (var variant in product.ProductVariants)
                {
                    variant.Price = newPrice;
                }
            }

            updateRepository.Update(product);
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ([.. allProducts.Select(p => p.Id)], null);
    }
}
