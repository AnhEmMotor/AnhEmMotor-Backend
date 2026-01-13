using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;

using MediatR;

namespace Application.Features.Products.Commands.UpdateManyVariantPrices;

public sealed class UpdateManyVariantPricesCommandHandler(
    IProductVariantReadRepository readRepository,
    IProductVariantUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyVariantPricesCommand, Result<List<int>>>
{
    public async Task<Result<List<int>>> Handle(
        UpdateManyVariantPricesCommand command,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();
        var variantIds = command.Ids;
        var newPrice = command.Price;

        var allVariants = await readRepository.GetByIdAsync(variantIds!, cancellationToken).ConfigureAwait(false);

        if(allVariants.ToList().Count != variantIds!.Count)
        {
            var foundIds = allVariants.Select(v => v.Id).ToHashSet();
            var missingIds = variantIds.Where(id => !foundIds.Contains(id)).ToList();

            foreach(var missingId in missingIds)
            {
                errors.Add(Error.NotFound($"Biến thể với Id {missingId} không tồn tại.", missingId.ToString()));
            }
        }

        if(errors.Count > 0)
        {
            return errors;
        }

        foreach(var variant in allVariants)
        {
            variant.Price = newPrice;
            updateRepository.Update(variant);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return variantIds;
    }
}
