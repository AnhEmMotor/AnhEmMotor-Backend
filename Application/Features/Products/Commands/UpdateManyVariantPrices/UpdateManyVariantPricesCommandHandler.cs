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
        var variantIds = command.Ids;
        var newPrice = command.Price;

        var allVariants = await selectRepository.GetActiveVariants()
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (allVariants.Count != variantIds.Count)
        {
            var foundIds = allVariants.Select(v => v.Id).ToHashSet();
            var missingIds = variantIds.Where(id => !foundIds.Contains(id)).ToList();
            
            foreach (var missingId in missingIds)
            {
                errors.Add(new ErrorDetail
                {
                    Field = missingId.ToString(),
                    Message = $"Biến thể với Id {missingId} không tồn tại."
                });
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        foreach (var variant in allVariants)
        {
            variant.Price = newPrice;
            updateRepository.UpdateVariant(variant);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (variantIds, null);
    }
}
