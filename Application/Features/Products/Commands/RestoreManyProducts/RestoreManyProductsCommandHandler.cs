using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Enums;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed class RestoreManyProductsCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyProductsCommand, (List<ApiContracts.Product.Responses.ProductDetailResponse>? Data, ErrorResponse? Error)>
{
    public async Task<(List<ApiContracts.Product.Responses.ProductDetailResponse>? Data, ErrorResponse? Error)> Handle(
        RestoreManyProductsCommand command,
        CancellationToken cancellationToken)
    {
        if(command.Ids == null || command.Ids.Count == 0)
        {
            return ([], null);
        }

        var uniqueIds = command.Ids.Distinct().ToList();

        var deletedProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);
        var allProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);

        var allProductsMap = allProducts.ToDictionary(p => p.Id);
        var deletedProductsSet = deletedProducts.Select(p => p.Id).ToHashSet();

        var errorDetails = new List<ErrorDetail>();

        foreach(var id in uniqueIds)
        {
            if(!allProductsMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail { Message = "Product not found", Field = $"Product ID: {id}" });
                continue;
            }

            if(!deletedProductsSet.Contains(id))
            {
                var productName = allProductsMap[id].Name;
                errorDetails.Add(new ErrorDetail { Message = "Product is not deleted", Field = productName });
            }
        }

        if(errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        if(deletedProducts.ToList().Count > 0)
        {
            updateRepository.Restore(deletedProducts);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var responses = deletedProducts.Select(p => p.Adapt<ApiContracts.Product.Responses.ProductDetailResponse>())
            .ToList();
        return (responses, null);
    }
}
