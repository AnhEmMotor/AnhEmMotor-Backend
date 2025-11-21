using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateVariantPrice;

public sealed class UpdateVariantPriceCommandHandler(
    IProductSelectRepository selectRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateVariantPriceCommand, (ProductVariantLiteResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductVariantLiteResponse? Data, ErrorResponse? Error)> Handle(UpdateVariantPriceCommand command, CancellationToken cancellationToken)
    {
        var variant = await selectRepository.GetActiveVariants()
            .Include(v => v.Product)
            .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                    .ThenInclude(ov => ov!.Option)
            .Include(v => v.InputInfos)
            .FirstOrDefaultAsync(v => v.Id == command.VariantId, cancellationToken)
            .ConfigureAwait(false);

        if (variant == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Biến thể với Id {command.VariantId} không tồn tại." }]
            });
        }

        variant.Price = command.Price;
        updateRepository.UpdateVariant(variant);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var optionPairs = variant.VariantOptionValues
            .Select(vov => new OptionPair
            {
                OptionName = vov.OptionValue?.Option?.Name,
                OptionValue = vov.OptionValue?.Name
            })
            .ToList();

        var stock = variant.InputInfos?.Sum(ii => ii.RemainingCount) ?? 0;

        var response = ProductResponseMapper.BuildVariantLiteResponse(
            variant.Id,
            variant.Product?.Id,
            variant.Product?.Name,
            optionPairs,
            variant.Price,
            variant.CoverImageUrl,
            stock
        );

        return (response, null);
    }
}
