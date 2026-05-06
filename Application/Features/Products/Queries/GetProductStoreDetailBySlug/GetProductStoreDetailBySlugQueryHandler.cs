using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Mapster;
using MediatR;
using System.Net;

namespace Application.Features.Products.Queries.GetProductStoreDetailBySlug;

public sealed class GetProductStoreDetailBySlugQueryHandler(IProductReadRepository productReadRepository) : IRequestHandler<GetProductStoreDetailBySlugQuery, Result<ProductStoreDetailResponse>>
{
    public async Task<Result<ProductStoreDetailResponse>> Handle(
        GetProductStoreDetailBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var decodedSlug = WebUtility.UrlDecode(request.Slug);
        var variant = await productReadRepository.GetByVariantSlugWithDetailsAsync(decodedSlug, cancellationToken)
            .ConfigureAwait(false);
        if (variant is null || variant.Product is null)
        {
            return Result<ProductStoreDetailResponse>.Failure(
                new Error("ProductDetail.NotFound", "Không tìm thấy sản phẩm."));
        }

        var product = variant.Product;
        var productResponse = product.Adapt<ProductInfoStoreResponse>();
        var currentVariantResponse = variant.Adapt<CurrentVariantStoreResponse>();

        var otherVariants = product.ProductVariants
            .Where(v => v.Id != variant.Id)
            .Adapt<List<OtherVariantStoreResponse>>();

        return Result<ProductStoreDetailResponse>.Success(
            new ProductStoreDetailResponse
            {
                Product = productResponse,
                CurrentVariant = currentVariantResponse,
                OtherVariants = otherVariants
            });
    }
}
