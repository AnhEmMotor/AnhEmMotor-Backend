using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using Domain.Constants;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public sealed class UpdateManyProductStatusesCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyProductStatusesCommand, Result<List<int>?>>
{
    public async Task<Result<List<int>?>> Handle(
        UpdateManyProductStatusesCommand command,
        CancellationToken cancellationToken)
    {
        var productIds = command.Ids!.Distinct().ToList();

        var products = await readRepository.GetByIdAsync(productIds, cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        var productList = products.ToList();

        if(productList.Count != productIds.Count)
        {
            var foundIds = productList.Select(p => p.Id).ToHashSet();
            var missingErrors = productIds
                .Where(id => !foundIds.Contains(id))
                .Select(id => Error.NotFound($"Sản phẩm với Id {id} không tồn tại."))
                .ToList();

            return missingErrors;
        }

        foreach(var product in productList)
        {
            product.StatusId = command.StatusId;
            updateRepository.Update(product);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return productIds;
    }
}