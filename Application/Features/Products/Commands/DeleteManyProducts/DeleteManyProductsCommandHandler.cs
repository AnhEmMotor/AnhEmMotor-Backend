using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using Domain.Constants;
using MediatR;

namespace Application.Features.Products.Commands.DeleteManyProducts;

public sealed class DeleteManyProductsCommandHandler(
    IProductReadRepository readRepository,
    IProductDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyProductsCommand, Common.Models.ErrorResponse?>
{
    public async Task<Common.Models.ErrorResponse?> Handle(
        DeleteManyProductsCommand command,
        CancellationToken cancellationToken)
    {
        if(command.Ids == null || command.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = command.Ids.Distinct().ToList();

        var activeProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        var allProductsMap = allProducts.ToDictionary(p => p.Id!);
        var activeProductsSet = activeProducts.Select(p => p.Id!).ToHashSet();

        var errorDetails = new List<Common.Models.ErrorDetail>();

        foreach(var id in uniqueIds)
        {
            if(!allProductsMap.ContainsKey(id))
            {
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Message = "Product not found", Field = $"Product ID: {id}" });
                continue;
            }

            if(!activeProductsSet.Contains(id))
            {
                var productName = allProductsMap[id].Name;
                errorDetails.Add(
                    new Common.Models.ErrorDetail { Message = "Product has already been deleted", Field = productName });
            }
        }

        if(errorDetails.Count > 0)
        {
            return new Common.Models.ErrorResponse { Errors = errorDetails };
        }

        if(activeProducts.ToList().Count > 0)
        {
            deleteRepository.Delete(activeProducts);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}
