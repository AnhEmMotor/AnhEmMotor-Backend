using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;

using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.UpdateVariantPrice;

public sealed class UpdateVariantPriceCommandHandler(
    IProductVariantReadRepository variantReadRepository,
    IProductVariantUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateVariantPriceCommand, Result<ProductVariantLiteResponse?>>
{
    public async Task<Result<ProductVariantLiteResponse?>> Handle(
        UpdateVariantPriceCommand command,
        CancellationToken cancellationToken)
    {
        var variant = await variantReadRepository.GetByIdWithDetailsAsync(command.VariantId, cancellationToken)
            .ConfigureAwait(false);

        if(variant == null)
        {
            return Error.NotFound($"Biến thể với Id {command.VariantId} không tồn tại.");
        }

        variant.Price = command.Price;
        updateRepository.Update(variant);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = variant.Adapt<ProductVariantLiteResponse>();
        return response;
    }
}