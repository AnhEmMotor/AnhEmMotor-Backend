using Application.ApiContracts.Product.Create;
using Domain.Helpers;

namespace Application.Interfaces.Services.Product;

public interface IProductInsertService
{
    Task<(ApiContracts.Product.Select.ProductDetailResponse? Data, ErrorResponse? Error)> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
}
