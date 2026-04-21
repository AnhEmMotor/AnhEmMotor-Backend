using Application.ApiContracts.Product.Responses;
using Application.Common.Helper;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductVariant;
using MediatR;
using System.Net;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed class CheckSlugAvailabilityQueryHandler(IProductVariantReadRepository readRepository) : IRequestHandler<CheckSlugAvailabilityQuery, Result<SlugAvailabilityResponse>>
{
    public async Task<Result<SlugAvailabilityResponse>> Handle(
        CheckSlugAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var decodedSlug = WebUtility.UrlDecode(request.Slug ?? string.Empty);
        var normalizedSlug = SlugHelper.GenerateSlug(decodedSlug);

        if(string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return new SlugAvailabilityResponse { Slug = normalizedSlug, IsAvailable = false };
        }

        var existing = await readRepository.GetBySlugAsync(normalizedSlug, cancellationToken).ConfigureAwait(false);

        return new SlugAvailabilityResponse { Slug = normalizedSlug, IsAvailable = existing == null };
    }
}
