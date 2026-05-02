using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductVariant;
using MediatR;

namespace Application.Features.Products.Queries.GetSitemapSlugs;

public sealed class GetSitemapSlugsQueryHandler(IProductVariantReadRepository productVariantReadRepository) : IRequestHandler<GetSitemapSlugsQuery, Result<SitemapSlugsResponse>>
{
    public async Task<Result<SitemapSlugsResponse>> Handle(
        GetSitemapSlugsQuery request,
        CancellationToken cancellationToken)
    {
        var productSlugs = await productVariantReadRepository.GetUrlSlugsAsync(cancellationToken).ConfigureAwait(false);

        return Result<SitemapSlugsResponse>.Success(new SitemapSlugsResponse { ProductSlugs = productSlugs });
    }
}
