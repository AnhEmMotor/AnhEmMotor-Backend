using Application.ApiContracts.Product.Get;
using Application.ApiContracts.Product.ManagePrices;
using Application.ApiContracts.Product.ManageStatus;
using Application.ApiContracts.Product.Update;
using Domain.Helpers;

namespace Application.Interfaces.Services.Product;

public interface IProductUpdateService
{
    Task<(ProductDetailResponse? Data, ErrorResponse? Error)> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<ErrorResponse?> UpdateProductPriceAsync(int id, UpdateProductPriceRequest request, CancellationToken cancellationToken);
    Task<ErrorResponse?> UpdateManyProductPricesAsync(UpdateManyProductPricesRequest request, CancellationToken cancellationToken);
    Task<ErrorResponse?> UpdateProductStatusAsync(int id, UpdateProductStatusRequest request, CancellationToken cancellationToken);
    Task<ErrorResponse?> UpdateManyProductStatusesAsync(UpdateManyProductStatusesRequest request, CancellationToken cancellationToken);
}
