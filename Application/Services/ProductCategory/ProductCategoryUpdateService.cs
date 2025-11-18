using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services.ProductCategory;
using Domain.Helpers;
using System.Linq;

namespace Application.Services.ProductCategory
{
    public class ProductCategoryUpdateService(IProductCategorySelectRepository selectRepository, IProductCategoryUpdateRepository updateRepository) : IProductCategoryUpdateService
    {
        public async Task<ErrorResponse?> UpdateAsync(int id, UpdateProductCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = await selectRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return new ErrorResponse
                {
                    Errors = [new ErrorDetail { Message = $"Product category with Id {id} not found." }]
                };
            }

            if (request.Name is not null)
            {
                category.Name = request.Name;
            }

            if (request.Description is not null)
            {
                category.Description = request.Description;
            }

            await updateRepository.UpdateAsync(category, cancellationToken).ConfigureAwait(false);
            return null;
        }

        public async Task<ErrorResponse?> RestoreAsync(int id, CancellationToken cancellationToken)
        {
            var deletedCategories = await selectRepository.GetDeletedCategoriesByIdsAsync([id], cancellationToken).ConfigureAwait(false);
            if (deletedCategories.Count == 0)
            {
                return new ErrorResponse
                {
                    Errors = [new ErrorDetail { Message = $"Deleted product category with Id {id} not found." }]
                };
            }

            await updateRepository.RestoreAsync(deletedCategories[0], cancellationToken).ConfigureAwait(false);
            return null;
        }

        public async Task<ErrorResponse?> RestoreManyAsync(RestoreManyProductCategoriesRequest request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();
            var deletedCategories = await selectRepository.GetDeletedCategoriesByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);
            var allCategories = await selectRepository.GetAllCategoriesByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);

            foreach (var id in request.Ids)
            {
                var category = allCategories.FirstOrDefault(c => c.Id == id);
                var deletedCategory = deletedCategories.FirstOrDefault(c => c.Id == id);

                if (category is null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Product category not found",
                        Field = $"ProductCategory ID: {id}"
                    });
                }
                else if (deletedCategory is null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Product category has already been restored",
                        Field = category.Name
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return new ErrorResponse { Errors = errorDetails };
            }

            if (deletedCategories.Count > 0)
            {
                await updateRepository.RestoreAsync(deletedCategories, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
    }
}
