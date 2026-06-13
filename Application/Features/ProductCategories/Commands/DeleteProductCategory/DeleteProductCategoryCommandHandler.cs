using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services;

using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteProductCategory;

public class DeleteProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryDeleteRepository deleteRepository,
    IProtectedProductCategoryService protectedCategoryService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (category is null || category.DeletedAt is not null)
        {
            return Result.Failure(Error.NotFound($"Product category with Id {request.Id} not found."));
        }
        var isProtected = await protectedCategoryService.IsProtectedAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (isProtected)
        {
            return Result.Failure(
                Error.Validation(
                    $"Cannot delete product category '{category.Name}'. This category is protected and cannot be deleted."));
        }
        var hasProducts = await readRepository.AnyCategoryInTreeHasProductsAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (hasProducts)
        {
            return Result.Failure(
                Error.Validation(
                    $"Cannot delete category '{category.Name}' or its subcategories because they still contain products."));
        }
        var subCategories = await readRepository.GetSubCategoriesAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (subCategories is { Count: > 0 })
        {
            deleteRepository.Delete(subCategories);
        }
        deleteRepository.Delete(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
