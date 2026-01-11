using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductPrice;

public sealed class UpdateProductPriceCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductPriceCommand, Result<ProductDetailForManagerResponse?>>
{
    public async Task<Result<ProductDetailForManagerResponse?>> Handle(
        UpdateProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdWithDetailsAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if(product == null)
        {
            return Error.NotFound($"Sản phẩm với Id {command.Id} không tồn tại.");
        }

        if(product.ProductVariants != null)
        {
            foreach(var variant in product.ProductVariants)
            {
                variant.Price = command.Price;
            }
        }

        updateRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = product.Adapt<ProductDetailForManagerResponse>();
        return response;
    }
}
