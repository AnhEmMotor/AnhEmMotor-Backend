using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.VariantOptionValue;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductPrice;

public sealed class UpdateProductPriceCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductPriceCommand, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(UpdateProductPriceCommand command, CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdWithDetailsAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Sản phẩm với Id {command.Id} không tồn tại." }]
            });
        }

        if (product.ProductVariants != null)
        {
            foreach (var variant in product.ProductVariants)
            {
                variant.Price = command.Price;
            }
        }

        updateRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = ProductResponseMapper.BuildProductDetailResponse(product);
        return (response, null);
    }
}
