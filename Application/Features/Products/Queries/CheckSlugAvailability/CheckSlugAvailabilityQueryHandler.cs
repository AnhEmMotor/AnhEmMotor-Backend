using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductVariant;
using MediatR;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed class CheckSlugAvailabilityQueryHandler(IProductVariantReadRepository readRepository) : IRequestHandler<CheckSlugAvailabilityQuery, Result<SlugAvailabilityResponse>>
{
    public async Task<Result<SlugAvailabilityResponse>> Handle(
        CheckSlugAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug?.Trim() ?? string.Empty;

        if(string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return new SlugAvailabilityResponse
            {
                Slug = normalizedSlug,
                IsAvailable = false
            };
        }

        var existing = await readRepository.GetBySlugAsync(normalizedSlug, cancellationToken).ConfigureAwait(false);

        return new SlugAvailabilityResponse
        {
            Slug = normalizedSlug,
            IsAvailable = existing == null
        };
    }
}
