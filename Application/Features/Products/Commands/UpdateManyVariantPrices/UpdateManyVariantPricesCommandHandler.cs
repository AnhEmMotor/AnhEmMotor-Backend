using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateManyVariantPrices;

public sealed class UpdateManyVariantPricesCommandHandler(
    IProductSelectRepository selectRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateManyVariantPricesCommand, (List<int>? Data, ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, ErrorResponse? Error)> Handle(UpdateManyVariantPricesCommand command, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var variantIds = command.VariantPrices.Keys.ToList();

        var allVariants = await selectRepository.GetActiveVariants()
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var variantMap = allVariants.ToDictionary(v => v.Id);

        foreach (var kvp in command.VariantPrices)
        {
            var variantId = kvp.Key;

            if (!variantMap.TryGetValue(variantId, out var variant))
            {
                errors.Add(new ErrorDetail
                {
                    Field = variantId.ToString(),
                    Message = $"Biến thể với Id {variantId} không tồn tại."
                });
                continue;
            }

            variant.Price = kvp.Value;
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        foreach (var variant in allVariants)
        {
            updateRepository.UpdateVariant(variant);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (variantIds, null);
    }
}
