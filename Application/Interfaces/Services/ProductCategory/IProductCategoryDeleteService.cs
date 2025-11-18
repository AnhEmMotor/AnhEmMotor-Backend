using Application.ApiContracts.ProductCategory;
using Domain.Helpers;

namespace Application.Interfaces.Services.ProductCategory
{
    public interface IProductCategoryDeleteService
    {
        Task<ErrorResponse?> DeleteAsync(int id, CancellationToken cancellationToken);
        Task<ErrorResponse?> DeleteManyAsync(DeleteManyProductCategoriesRequest request, CancellationToken cancellationToken);
    }
}
