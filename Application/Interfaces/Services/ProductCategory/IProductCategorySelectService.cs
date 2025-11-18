using Application.ApiContracts.ProductCategory;
using Domain.Helpers;
using Sieve.Models;

namespace Application.Interfaces.Services.ProductCategory
{
    public interface IProductCategorySelectService
    {
        Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<PagedResult<ProductCategoryResponse>> GetProductCategoriesAsync(SieveModel sieveModel, CancellationToken cancellationToken);
        Task<PagedResult<ProductCategoryResponse>> GetDeletedProductCategoriesAsync(SieveModel sieveModel, CancellationToken cancellationToken);
    }
}
