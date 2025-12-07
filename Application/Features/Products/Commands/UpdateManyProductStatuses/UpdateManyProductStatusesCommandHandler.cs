using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Constants;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public sealed class UpdateManyProductStatusesCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyProductStatusesCommand, (List<int>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, Common.Models.ErrorResponse? Error)> Handle(
        UpdateManyProductStatusesCommand command,
        CancellationToken cancellationToken)
    {
        var productIds = command.Ids.Distinct().ToList();

        var products = await readRepository.GetByIdAsync(productIds, cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        var productList = products.ToList();

        if(productList.Count != productIds.Count)
        {
            var foundIds = productList.Select(p => p.Id).ToHashSet();
            var missingErrors = productIds
                .Where(id => !foundIds.Contains(id))
                .Select(
                    id => new Common.Models.ErrorDetail
                    {
                        Field = id.ToString(),
                        Message = $"Sản phẩm với Id {id} không tồn tại."
                    })
                .ToList();

            return (null, new Common.Models.ErrorResponse { Errors = missingErrors });
        }

        foreach(var product in productList)
        {
            product.StatusId = command.StatusId;
            updateRepository.Update(product);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (productIds, null);
    }
}