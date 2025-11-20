using Application.ApiContracts.Product.Delete;
using Domain.Helpers;

namespace Application.Interfaces.Services.Product
{
    public interface IProductDeleteService
    {
        Task<ErrorResponse?> DeleteProductAsync(int id, CancellationToken cancellationToken);
        Task<ErrorResponse?> DeleteProductsAsync(DeleteManyProductsRequest request, CancellationToken cancellationToken);
    }
}
