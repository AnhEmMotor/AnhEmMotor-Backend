using Application.ApiContracts.Product.Common;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed class CheckSlugAvailabilityQueryHandler(IProductSelectRepository selectRepository)
    : IRequestHandler<CheckSlugAvailabilityQuery, SlugAvailabilityResponse>
{
    public async Task<SlugAvailabilityResponse> Handle(
        CheckSlugAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return new SlugAvailabilityResponse
            {
                Slug = normalizedSlug,
                IsAvailable = false
            };
        }

        var existing = await selectRepository
            .GetVariantBySlugAsync(normalizedSlug, includeDeleted: true, cancellationToken)
            .ConfigureAwait(false);

        return new SlugAvailabilityResponse
        {
            Slug = normalizedSlug,
            IsAvailable = existing == null
        };
    }
}
