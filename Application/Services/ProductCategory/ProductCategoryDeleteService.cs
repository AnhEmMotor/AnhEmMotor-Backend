using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services.ProductCategory;
using Domain.Helpers;

namespace Application.Services.ProductCategory;

public class ProductCategoryDeleteService(
    IProductCategorySelectRepository selectRepository,
    IProductCategoryDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IProductCategoryDeleteService
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

        deleteRepository.Delete(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }

    public async Task<ErrorResponse?> DeleteManyAsync(DeleteManyProductCategoriesRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = request.Ids.Distinct().ToList();

        var activeCategories = await selectRepository.GetActiveCategoriesByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allCategories = await selectRepository.GetAllCategoriesByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allCategoriesMap = allCategories.ToDictionary(c => c.Id);
        var activeCategoriesSet = activeCategories.Select(c => c.Id).ToHashSet();

        var errorDetails = new List<ErrorDetail>();

        foreach (var id in uniqueIds)
        {
            if (!allCategoriesMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Message = "Product category not found",
                    Field = $"ProductCategory ID: {id}"
                });
                continue;
            }

            if (!activeCategoriesSet.Contains(id))
            {
                var categoryName = allCategoriesMap[id].Name;
                errorDetails.Add(new ErrorDetail
                {
                    Message = "Product category has already been deleted",
                    Field = categoryName
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        if (activeCategories.Count > 0)
        {
            deleteRepository.Delete(activeCategories);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}