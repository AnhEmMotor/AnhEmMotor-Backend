using Application.ApiContracts.Product.Create;
using Application.ApiContracts.Product.Get;
using Domain.Helpers;

namespace Application.Interfaces.Services.Product;

public interface IProductInsertService
{
    Task<(ProductDetailResponse? Data, ErrorResponse? Error)> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<(ProductDetailResponse? Data, ErrorResponse? Error)> UpsertProductAsync(UpsertProductRequest request, CancellationToken cancellationToken);
}
