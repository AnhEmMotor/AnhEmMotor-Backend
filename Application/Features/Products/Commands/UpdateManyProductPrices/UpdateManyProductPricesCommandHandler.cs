using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.VariantOptionValue;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed class UpdateManyProductPricesCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateManyProductPricesCommand, (List<int>? Data, ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, ErrorResponse? Error)> Handle(UpdateManyProductPricesCommand command, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var productIds = command.Ids;
        var newPrice = command.Price;

        var productsWithVarients = await readRepository.GetByIdWithVariantsAsync(productIds, cancellationToken);

        if (productsWithVarients.ToList().Count != productIds.Count)
        {
            var foundIds = productsWithVarients.Select(p => p.Id).ToHashSet();
            var missingIds = productIds.Where(id => !foundIds.Contains(id)).ToList();
            
            foreach (var missingId in missingIds)
            {
                errors.Add(new ErrorDetail
                {
                    Field = missingId.ToString(),
                    Message = $"Sản phẩm với Id {missingId} không tồn tại."
                });
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        foreach (var product in productsWithVarients)
        {
            if (product.ProductVariants != null)
            {
                foreach (var variant in product.ProductVariants)
                {
                    variant.Price = newPrice;
                }
            }

            updateRepository.Update(product);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        //Chổ này, phải trả về danh sách sản phẩm chứ không phải danh sách ID!
        return ([.. productsWithVarients.Select(p => p.Id)], null);
    }
}
