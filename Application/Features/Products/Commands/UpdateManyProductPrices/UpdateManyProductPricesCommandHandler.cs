using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed class UpdateManyProductPricesCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyProductPricesCommand, (List<int>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, Common.Models.ErrorResponse? Error)> Handle(
        UpdateManyProductPricesCommand command,
        CancellationToken cancellationToken)
    {
        var productIds = command.Ids.Distinct().ToList();

        var products = await readRepository.GetByIdWithVariantsAsync(productIds, cancellationToken)
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
            if(product.ProductVariants != null)
            {
                foreach(var variant in product.ProductVariants)
                {
                    variant.Price = command.Price;
                }
            }

            updateRepository.Update(product);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (productIds, null);
    }
}