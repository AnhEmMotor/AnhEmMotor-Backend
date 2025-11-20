using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services.ProductCategory;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Services.ProductCategory
{
    public class ProductCategoryInsertService(IProductCategoryInsertRepository insertRepository, IUnitOfWork unitOfWork) : IProductCategoryInsertService
    {
        public async Task<ProductCategoryResponse> CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = new CategoryEntity
            {
                Name = request.Name,
                Description = request.Description
            };

            await insertRepository.AddAsync(category, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);


            return new ProductCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }
    }
}
