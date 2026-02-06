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
    public async Task<Result> Handle(DeleteManyProductsCommand command, CancellationToken cancellationToken)
    {
        var uniqueIds = command.Ids!.Distinct().ToList();

        var allProducts = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);

        var allProductsMap = allProducts.ToDictionary(p => p.Id!);
        var errorDetails = new List<Error>();

        foreach(var id in uniqueIds)
        {
            if(!allProductsMap.TryGetValue(id, out var product))
            {
                errorDetails.Add(Error.NotFound($"Product not found, Product ID: {id}"));
                continue;
            }

            if(product.DeletedAt != null)
            {
                errorDetails.Add(
                    Error.BadRequest(
                        $"Product has already been deleted, Product ID: {id}, Product Name: {product.Name}"));
            }
        }

        if(errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }

        var productsToDelete = allProductsMap.Values.Where(p => p.DeletedAt == null).ToList();

        if(productsToDelete.Count > 0)
        {
            deleteRepository.Delete(productsToDelete);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}
