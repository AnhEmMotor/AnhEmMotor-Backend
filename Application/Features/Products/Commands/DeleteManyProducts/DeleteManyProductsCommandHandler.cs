using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using Domain.Constants;
using MediatR;

namespace Application.Features.Products.Commands.DeleteManyProducts;

public sealed class DeleteManyProductsCommandHandler(
    IProductReadRepository readRepository,
    IProductDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyProductsCommand, Result>
{
    public async Task<Result> Handle(
        DeleteManyProductsCommand command,
        CancellationToken cancellationToken)
    {
        var uniqueIds = command.Ids!.Distinct().ToList();

        var activeProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        var allProductsMap = allProducts.ToDictionary(p => p.Id!);
        var activeProductsSet = activeProducts.Select(p => p.Id!).ToHashSet();

        var errorDetails = new List<Error>();

        foreach(var id in uniqueIds)
        {
            if(!allProductsMap.ContainsKey(id))
            {
                errorDetails.Add(Error.NotFound($"Product not found, Product ID: {id}"));
            }

            if(!activeProductsSet.Contains(id))
            {
                var productName = allProductsMap[id].Name;
                errorDetails.Add(Error.BadRequest($"Product has already been deleted, Product ID: {id}, Product Name: {productName}"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }

        if(activeProducts.ToList().Count > 0)
        {
            deleteRepository.Delete(activeProducts);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}
