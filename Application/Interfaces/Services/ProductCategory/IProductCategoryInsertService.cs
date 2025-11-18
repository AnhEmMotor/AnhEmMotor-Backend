using Application.ApiContracts.ProductCategory;

namespace Application.Interfaces.Services.ProductCategory
{
    public interface IProductCategoryInsertService
    {
        Task<ProductCategoryResponse> CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken);
    }
}
