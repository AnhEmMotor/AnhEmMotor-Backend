using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed class UpdateManyProductPricesCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyProductPricesCommand, Result<List<int>?>>
{
    public async Task<Result<List<int>?>> Handle(
        UpdateManyProductPricesCommand command,
        CancellationToken cancellationToken)
    {
        var productIds = command.Ids!.Distinct().ToList();

        var products = await readRepository.GetByIdWithVariantsAsync(productIds, cancellationToken)
            .ConfigureAwait(false);
        var productList = products.ToList();

        if(productList.Count != productIds.Count)
        {
            var foundIds = productList.Select(p => p.Id).ToHashSet();
            var missingErrors = productIds
                .Where(id => !foundIds.Contains(id))
                .Select(
                    id => Error.NotFound($"Sản phẩm với Id {id} không tồn tại."))
                .ToList();

            return missingErrors;
        }

        foreach(var product in productList)
        {
            if(product.ProductVariants != null)
            {
                foreach(var variant in product.ProductVariants)
                {
                    variant.Price = command.Price;
                }
            }

            updateRepository.Update(product);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return productIds;
    }
}