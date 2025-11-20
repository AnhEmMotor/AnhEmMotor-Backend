using Application.ApiContracts.Product.Delete;
using Application.ApiContracts.Product.Update;
using Domain.Helpers;

namespace Application.Interfaces.Services.Product;

public interface IProductUpdateService
{
    Task<(ApiContracts.Product.Select.ProductDetailResponse? Data, ErrorResponse? Error)> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<(ApiContracts.Product.Select.ProductDetailResponse? Data, ErrorResponse? Error)> UpdateProductPriceAsync(int id, UpdateProductPriceRequest request, CancellationToken cancellationToken);
    Task<(List<int>? Data, ErrorResponse? Error)> UpdateManyProductPricesAsync(UpdateManyProductPricesRequest request, CancellationToken cancellationToken);
    Task<(ApiContracts.Product.Select.ProductDetailResponse? Data, ErrorResponse? Error)> UpdateProductStatusAsync(int id, UpdateProductStatusRequest request, CancellationToken cancellationToken);
    Task<(List<int>? Data, ErrorResponse? Error)> UpdateManyProductStatusesAsync(UpdateManyProductStatusesRequest request, CancellationToken cancellationToken);
    Task<(ApiContracts.Product.Select.ProductDetailResponse? Data, ErrorResponse? Error)> RestoreProductAsync(int id, CancellationToken cancellationToken);
    Task<(List<int>? Data, ErrorResponse? Error)> RestoreProductsAsync(RestoreManyProductsRequest request, CancellationToken cancellationToken);
}
