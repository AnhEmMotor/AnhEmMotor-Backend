using Application.ApiContracts.Product.Common;
using Application.Interfaces.Repositories.ProductVariant;
using MediatR;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed class CheckSlugAvailabilityQueryHandler(IProductVariantReadRepository readRepository) : IRequestHandler<CheckSlugAvailabilityQuery, SlugAvailabilityResponse>
{
    public async Task<SlugAvailabilityResponse> Handle(
        CheckSlugAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug?.Trim() ?? string.Empty;

        if(string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return new SlugAvailabilityResponse { Slug = normalizedSlug, IsAvailable = false };
        }

        var existing = await readRepository.GetBySlugAsync(normalizedSlug, cancellationToken).ConfigureAwait(false);

        return new SlugAvailabilityResponse { Slug = normalizedSlug, IsAvailable = existing == null };
    }
}
