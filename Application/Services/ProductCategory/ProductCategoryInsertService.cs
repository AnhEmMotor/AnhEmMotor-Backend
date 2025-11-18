using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services.ProductCategory;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Services.ProductCategory
{
    public class ProductCategoryInsertService(IProductCategoryInsertRepository insertRepository) : IProductCategoryInsertService
    {
        public async Task<ProductCategoryResponse> CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = new CategoryEntity
            {
                Name = request.Name,
                Description = request.Description
            };

            var created = await insertRepository.AddAsync(category, cancellationToken).ConfigureAwait(false);

            return new ProductCategoryResponse
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description
            };
        }
    }
}
