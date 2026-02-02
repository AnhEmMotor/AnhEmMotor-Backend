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

        var allProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);

        var allProductsMap = allProducts.ToDictionary(p => p.Id);
        var errorDetails = new List<Error>();
        var productsToRestore = new List<Domain.Entities.Product>();

        foreach (var id in uniqueIds)
        {
            if (!allProductsMap.TryGetValue(id, out var product))
            {
                errorDetails.Add(Error.NotFound($"Product not found, Product ID: {id}"));
                continue;
            }

            if (product.DeletedAt == null)
            {
                errorDetails.Add(
                    Error.BadRequest($"Product is not deleted, Product ID: {id}, Product Name: {product.Name}"));
                continue;
            }

            productsToRestore.Add(product);
        }

        if (errorDetails.Count > 0)
        {
            return Result<List<ProductDetailForManagerResponse>?>.Failure(errorDetails);
        }

        if (productsToRestore.Count > 0)
        {
            updateRepository.Restore(productsToRestore);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var responses = productsToRestore.Select(p => p.Adapt<ProductDetailForManagerResponse>()).ToList();
        return Result<List<ProductDetailForManagerResponse>?>.Success(responses);
    }
}
