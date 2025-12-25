using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed class RestoreManyProductsCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyProductsCommand, (List<ApiContracts.Product.Responses.ProductDetailForManagerResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<ApiContracts.Product.Responses.ProductDetailForManagerResponse>? Data, Common.Models.ErrorResponse? Error)> Handle(
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

        var errorDetails = new List<Common.Models.ErrorDetail>();

        foreach(var id in uniqueIds)
        {
            if(!allProductsMap.ContainsKey(id))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Message = "Product not found", Field = $"Product ID: {id}" });
                continue;
            }

            if(!deletedProductsSet.Contains(id))
            {
                var productName = allProductsMap[id].Name;
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Message = "Product is not deleted", Field = productName });
            }
        }

        if(errorDetails.Count > 0)
        {
            return (null, new Common.Models.ErrorResponse { Errors = errorDetails });
        }

        if(deletedProducts.ToList().Count > 0)
        {
            updateRepository.Restore(deletedProducts);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var responses = deletedProducts.Select(
            p => p.Adapt<ApiContracts.Product.Responses.ProductDetailForManagerResponse>())
            .ToList();
        return (responses, null);
    }
}
