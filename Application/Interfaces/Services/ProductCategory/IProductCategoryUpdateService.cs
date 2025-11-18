using Application.ApiContracts.ProductCategory;
using Domain.Helpers;

namespace Application.Interfaces.Services.ProductCategory
{
    public interface IProductCategoryUpdateService
    {
        Task<ErrorResponse?> UpdateAsync(int id, UpdateProductCategoryRequest request, CancellationToken cancellationToken);
        Task<ErrorResponse?> RestoreAsync(int id, CancellationToken cancellationToken);
        Task<ErrorResponse?> RestoreManyAsync(RestoreManyProductCategoriesRequest request, CancellationToken cancellationToken);
    }
}
