using Application.ApiContracts.Product.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.UpdateVariantPrice;

public sealed class UpdateVariantPriceCommandHandler(
    IProductVariantReadRepository variantReadRepository,
    IProductVariantUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateVariantPriceCommand, (ProductVariantLiteResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductVariantLiteResponse? Data, ErrorResponse? Error)> Handle(
        UpdateVariantPriceCommand command,
        CancellationToken cancellationToken)
    {
        var variant = await variantReadRepository.GetByIdWithDetailsAsync(command.VariantId, cancellationToken);

        if(variant == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = $"Biến thể với Id {command.VariantId} không tồn tại." } ]
            });
        }

        variant.Price = command.Price;
        updateRepository.Update(variant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = variant.Adapt<ProductVariantLiteResponse>();
        return (response, null);
    }
}