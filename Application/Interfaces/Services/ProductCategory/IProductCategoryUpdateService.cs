using Application.ApiContracts.ProductCategory;
using Domain.Helpers;

namespace Application.Interfaces.Services.ProductCategory
{
    public interface IProductCategoryUpdateService
    {
        Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> UpdateAsync(int id, UpdateProductCategoryRequest request, CancellationToken cancellationToken);
        Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> RestoreAsync(int id, CancellationToken cancellationToken);
        Task<(List<int>? Data, ErrorResponse? Error)> RestoreManyAsync(RestoreManyProductCategoriesRequest request, CancellationToken cancellationToken);
    }
}
