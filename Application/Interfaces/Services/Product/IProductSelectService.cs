using Application.ApiContracts.Product.Common;
using Domain.Helpers;

namespace Application.Interfaces.Services.Product
{
    public interface IProductSelectService
    {
        Task<PagedResult<ApiContracts.Product.Select.ProductDetailResponse>> GetProductsAsync(ApiContracts.Product.Select.ProductListRequest request, CancellationToken cancellationToken);
        Task<PagedResult<ApiContracts.Product.Select.ProductDetailResponse>> GetDeletedProductsAsync(ApiContracts.Product.Select.ProductListRequest request, CancellationToken cancellationToken);
        Task<PagedResult<ProductVariantLiteResponse>> GetActiveVariantLiteProductsAsync(ApiContracts.Product.Select.ProductListRequest request, CancellationToken cancellationToken);
        Task<PagedResult<ProductVariantLiteResponse>> GetDeletedVariantLiteProductsAsync(ApiContracts.Product.Select.ProductListRequest request, CancellationToken cancellationToken);
        Task<(ApiContracts.Product.Select.ProductDetailResponse? Data, ErrorResponse? Error)> GetProductDetailsByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken);
        Task<(List<ProductVariantLiteResponse> Variants, ErrorResponse? Error)> GetVariantLiteByProductIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken);
        Task<SlugAvailabilityResponse> CheckSlugAvailabilityAsync(string slug, CancellationToken cancellationToken);
    }
}
