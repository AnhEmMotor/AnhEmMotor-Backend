using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services;

using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteManyProductCategories;

public class DeleteManyProductCategoriesCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryDeleteRepository deleteRepository,
    IProtectedProductCategoryService protectedCategoryService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyProductCategoriesCommand, Result>
{
    public async Task<Result> Handle(DeleteManyProductCategoriesCommand request, CancellationToken cancellationToken)
    {
        var uniqueIds = request.Ids.Distinct().ToList();
        var errorDetails = new List<Error>();
        var allCategories = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        var activeCategories = await readRepository.GetByIdAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allCategoryMap = allCategories.ToDictionary(c => c.Id);
        var activeCategorySet = activeCategories.Select(c => c.Id).ToHashSet();
        foreach (var id in uniqueIds)
        {
            if (!allCategoryMap.TryGetValue(id, out var category))
            {
                errorDetails.Add(Error.NotFound($"Product category with Id {id} not found.", "Id"));
                continue;
            }
            if (!activeCategorySet.Contains(id))
            {
                errorDetails.Add(
                    Error.BadRequest($"Product category '{category.Name}' (Id {id}) has already been deleted.", "Id"));
                continue;
            }
            var isProtected = await protectedCategoryService.IsProtectedAsync(id, cancellationToken)
                .ConfigureAwait(false);
            if (isProtected)
            {
                errorDetails.Add(
                    Error.Validation($"Cannot delete product category '{category.Name}'. This category is protected."));
                continue;
            }
        }
        if (errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }
        var hasProducts = await readRepository.AnyInTreeHasProductsAsync(uniqueIds, cancellationToken)
            .ConfigureAwait(false);
        if (hasProducts)
        {
            return Result.Failure(
                Error.Validation(
                    "Cannot delete categories because some of them or their subcategories still contain products."));
        }
        if (activeCategories.Count > 0)
        {
            var allToBeDeleted = new List<ProductCategory>(activeCategories);
            foreach (var category in activeCategories)
            {
                var subCategories = await readRepository.GetSubCategoriesAsync(category.Id, cancellationToken)
                    .ConfigureAwait(false);
                foreach (var sub in subCategories)
                {
                    if (allToBeDeleted.All(c => c.Id != sub.Id))
                    {
                        allToBeDeleted.Add(sub);
                    }
                }
            }
            deleteRepository.Delete(allToBeDeleted);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        return Result.Success();
    }
}
