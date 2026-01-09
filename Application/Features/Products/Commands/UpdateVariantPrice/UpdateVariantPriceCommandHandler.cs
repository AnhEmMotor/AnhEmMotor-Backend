using Application.ApiContracts.Product.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Common.Models;
using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.UpdateVariantPrice;

public sealed class UpdateVariantPriceCommandHandler(
    IProductVariantReadRepository variantReadRepository,
    IProductVariantUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateVariantPriceCommand, (ProductVariantLiteResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(ProductVariantLiteResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        UpdateVariantPriceCommand command,
        CancellationToken cancellationToken)
    {
        var variant = await variantReadRepository.GetByIdWithDetailsAsync(command.VariantId, cancellationToken)
            .ConfigureAwait(false);

        if(variant == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail { Message = $"Biến thể với Id {command.VariantId} không tồn tại." } ]
            });
        }

        variant.Price = command.Price;
        updateRepository.Update(variant);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = variant.Adapt<ProductVariantLiteResponse>();
        return (response, null);
    }
}