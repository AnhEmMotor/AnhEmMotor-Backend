using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.DeleteManyProducts;

public sealed class DeleteManyProductsCommandHandler(
    IProductSelectRepository selectRepository,
    IProductDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteManyProductsCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteManyProductsCommand command, CancellationToken cancellationToken)
    {
        if (command.Ids == null || command.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = command.Ids.Distinct().ToList();

        var activeProducts = await selectRepository.GetActiveProductsByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allProducts = await selectRepository.GetAllProductsByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allProductsMap = allProducts.ToDictionary(p => p.Id!);
        var activeProductsSet = activeProducts.Select(p => p.Id!).ToHashSet();

        var errorDetails = new List<ErrorDetail>();

        foreach (var id in uniqueIds)
        {
            if (!allProductsMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Message = "Product not found",
                    Field = $"Product ID: {id}"
                });
                continue;
            }

            if (!activeProductsSet.Contains(id))
            {
                var productName = allProductsMap[id].Name;
                errorDetails.Add(new ErrorDetail
                {
                    Message = "Product has already been deleted",
                    Field = productName
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        if (activeProducts.Count > 0)
        {
            deleteRepository.Delete(activeProducts);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}
