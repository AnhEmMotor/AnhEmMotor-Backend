using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Common.Models;
using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductPrice;

public sealed class UpdateProductPriceCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductPriceCommand, (ApiContracts.Product.Responses.ProductDetailForManagerResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(ApiContracts.Product.Responses.ProductDetailForManagerResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        UpdateProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdWithDetailsAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if(product == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors = [ new Common.Models.ErrorDetail { Message = $"Sản phẩm với Id {command.Id} không tồn tại." } ]
            });
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

        var response = product.Adapt<ApiContracts.Product.Responses.ProductDetailForManagerResponse>();
        return (response, null);
    }
}
