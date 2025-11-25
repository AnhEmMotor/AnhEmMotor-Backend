using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyVariantPrices;

public sealed class UpdateManyVariantPricesCommandHandler(
    IProductVariantReadRepository readRepository,
    IProductVariantUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateManyVariantPricesCommand, (List<int>? Data, ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, ErrorResponse? Error)> Handle(
        UpdateManyVariantPricesCommand command,
        CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var variantIds = command.Ids;
        var newPrice = command.Price;

        var allVariants = await readRepository.GetByIdAsync(variantIds, cancellationToken).ConfigureAwait(false);

        if(allVariants.ToList().Count != variantIds.Count)
        {
            var foundIds = allVariants.Select(v => v.Id).ToHashSet();
            var missingIds = variantIds.Where(id => !foundIds.Contains(id)).ToList();

            foreach(var missingId in missingIds)
            {
                errors.Add(
                    new ErrorDetail
                    {
                        Field = missingId.ToString(),
                        Message = $"Biến thể với Id {missingId} không tồn tại."
                    });
            }
        }

        if(errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        foreach(var variant in allVariants)
        {
            variant.Price = newPrice;
            updateRepository.Update(variant);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (variantIds, null);
    }
}
