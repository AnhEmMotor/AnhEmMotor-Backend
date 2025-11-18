using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Get;
using Domain.Helpers;

namespace Application.Interfaces.Services.Product
{
    public interface IProductSelectService
    {
        Task<PagedResult<ProductDetailResponse>> GetProductsAsync(ProductListRequest request, CancellationToken cancellationToken);
        Task<PagedResult<ProductDetailResponse>> GetDeletedProductsAsync(ProductListRequest request, CancellationToken cancellationToken);
        Task<PagedResult<ProductVariantLiteResponse>> GetActiveVariantLiteProductsAsync(ProductListRequest request, CancellationToken cancellationToken);
        Task<PagedResult<ProductVariantLiteResponse>> GetDeletedVariantLiteProductsAsync(ProductListRequest request, CancellationToken cancellationToken);
        Task<(ProductDetailResponse? Data, ErrorResponse? Error)> GetProductDetailsByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken);
        Task<(List<ProductVariantLiteResponse> Variants, ErrorResponse? Error)> GetVariantLiteByProductIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken);
        Task<SlugAvailabilityResponse> CheckSlugAvailabilityAsync(string slug, CancellationToken cancellationToken);
    }
}
