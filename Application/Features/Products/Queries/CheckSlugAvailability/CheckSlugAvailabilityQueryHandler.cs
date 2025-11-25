using Application.Interfaces.Repositories.ProductVariant;
using MediatR;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed class CheckSlugAvailabilityQueryHandler(IProductVariantReadRepository readRepository) : IRequestHandler<CheckSlugAvailabilityQuery, ApiContracts.Product.Responses.SlugAvailabilityResponse>
{
    public async Task<ApiContracts.Product.Responses.SlugAvailabilityResponse> Handle(
        CheckSlugAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug?.Trim() ?? string.Empty;

        if(string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return new ApiContracts.Product.Responses.SlugAvailabilityResponse
            {
                Slug = normalizedSlug,
                IsAvailable = false
            };
        }

        var existing = await readRepository.GetBySlugAsync(normalizedSlug, cancellationToken).ConfigureAwait(false);

        return new ApiContracts.Product.Responses.SlugAvailabilityResponse
        {
            Slug = normalizedSlug,
            IsAvailable = existing == null
        };
    }
}
