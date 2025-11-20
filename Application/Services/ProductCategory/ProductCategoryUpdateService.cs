using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services.ProductCategory;
using Domain.Helpers;

namespace Application.Services.ProductCategory;

public class ProductCategoryUpdateService(
    IProductCategorySelectRepository selectRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IProductCategoryUpdateService
{
    public async Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> UpdateAsync(int id, UpdateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await selectRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (category == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Product category with Id {id} not found." }]
            });
        }

        if (request.Name is not null)
        {
            category.Name = request.Name;
        }

        if (request.Description is not null)
        {
            category.Description = request.Description;
        }

        updateRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new ProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        }, null);
    }

    public async Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var deletedCategories = await selectRepository.GetDeletedCategoriesByIdsAsync([id], cancellationToken).ConfigureAwait(false);

        if (deletedCategories.Count == 0)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Deleted product category with Id {id} not found." }]
            });
        }

        var category = deletedCategories[0];
        updateRepository.Restore(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new ProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        }, null);
    }

    public async Task<(List<int>? Data, ErrorResponse? Error)> RestoreManyAsync(RestoreManyProductCategoriesRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return ([], null);
        }

        var uniqueIds = request.Ids.Distinct().ToList();

        var deletedCategories = await selectRepository.GetDeletedCategoriesByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allCategories = await selectRepository.GetAllCategoriesByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allCategoriesMap = allCategories.ToDictionary(c => c.Id);
        var deletedCategoriesSet = deletedCategories.Select(c => c.Id).ToHashSet();

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

            if (!deletedCategoriesSet.Contains(id))
            {
                var categoryName = allCategoriesMap[id].Name;
                errorDetails.Add(new ErrorDetail
                {
                    Message = "Product category is not deleted",
                    Field = categoryName
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errorDetails });
        }

        if (deletedCategories.Count > 0)
        {
            updateRepository.Restore(deletedCategories);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return (uniqueIds, null);
    }
}