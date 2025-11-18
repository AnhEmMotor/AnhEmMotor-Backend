using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services.ProductCategory;
using Domain.Helpers;
using System.Linq;

namespace Application.Services.ProductCategory
{
    public class ProductCategoryDeleteService(IProductCategorySelectRepository selectRepository, IProductCategoryDeleteRepository deleteRepository) : IProductCategoryDeleteService
    {
        public async Task<ErrorResponse?> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var category = await selectRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return new ErrorResponse
                {
                    Errors = [new ErrorDetail { Message = $"Product category with Id {id} not found." }]
                };
            }

            await deleteRepository.DeleteAsync(category, cancellationToken).ConfigureAwait(false);
            return null;
        }

        public async Task<ErrorResponse?> DeleteManyAsync(DeleteManyProductCategoriesRequest request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();
            var activeCategories = await selectRepository.GetActiveCategoriesByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);
            var allCategories = await selectRepository.GetAllCategoriesByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);

            foreach (var id in request.Ids)
            {
                var category = allCategories.FirstOrDefault(c => c.Id == id);
                var activeCategory = activeCategories.FirstOrDefault(c => c.Id == id);

                if (category is null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Product category not found",
                        Field = $"ProductCategory ID: {id}"
                    });
                }
                else if (activeCategory is null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Product category has already been deleted",
                        Field = category.Name
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return new ErrorResponse { Errors = errorDetails };
            }

            if (activeCategories.Count > 0)
            {
                await deleteRepository.DeleteAsync(activeCategories, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
    }
}
