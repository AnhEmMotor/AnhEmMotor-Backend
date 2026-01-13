using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed class RestoreManyProductsCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyProductsCommand, Result<List<ProductDetailForManagerResponse>?>>
{
    public async Task<Result<List<ProductDetailForManagerResponse>?>> Handle(
        RestoreManyProductsCommand command,
        CancellationToken cancellationToken)
    {
        var uniqueIds = command.Ids!.Distinct().ToList();

        var deletedProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);
        var allProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);

        var allProductsMap = allProducts.ToDictionary(p => p.Id);
        var deletedProductsSet = deletedProducts.Select(p => p.Id).ToHashSet();

        var errorDetails = new List<Error>();

        foreach(var id in uniqueIds)
        {
            if(!allProductsMap.ContainsKey(id))
            {
                errorDetails.Add(Error.NotFound($"Product not found, Product ID: {id}"));
            }

            if(!deletedProductsSet.Contains(id))
            {
                var productName = allProductsMap[id].Name;
                errorDetails.Add(Error.BadRequest($"Product is not deleted, Product ID: {id}, Product Name: {productName}"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return errorDetails;
        }

        if(deletedProducts.ToList().Count > 0)
        {
            updateRepository.Restore(deletedProducts);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var responses = deletedProducts.Select(
            p => p.Adapt<ProductDetailForManagerResponse>())
            .ToList();
        return responses;
    }
}
