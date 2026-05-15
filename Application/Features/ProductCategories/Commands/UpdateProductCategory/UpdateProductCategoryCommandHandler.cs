using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed class UpdateProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCategoryCommand, Result<ProductCategoryResponse?>>
{
    public async Task<Result<ProductCategoryResponse?>> Handle(
        UpdateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (category == null || category.DeletedAt != null)
        {
            return Result<ProductCategoryResponse?>.Failure(
                Error.NotFound($"Product category with Id {request.Id} not found or has been deleted."));
        }
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var nameToUpdate = request.Name.Trim();
            var isDuplicate = await readRepository.ExistsByNameExceptIdAsync(
                nameToUpdate,
                request.Id,
                cancellationToken,
                DataFetchMode.All)
                .ConfigureAwait(false);
            if (isDuplicate)
            {
                return Result<ProductCategoryResponse?>.Failure(
                    Error.Conflict($"Category name '{nameToUpdate}' already exists."));
            }
            category.Name = nameToUpdate;
        }
        if (request.Description != null)
            category.Description = request.Description.Trim();
        if (request.Slug != null)
            category.Slug = request.Slug.Trim();
        if (request.ImageUrl != null)
            category.ImageUrl = request.ImageUrl.Trim();
        if (request.CategoryGroup != null)
            category.CategoryGroup = request.CategoryGroup.Trim();
        category.IsActive = request.IsActive;
        category.SortOrder = request.SortOrder;
        
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
            {
                return Result<ProductCategoryResponse?>.Failure(Error.Validation("A category cannot be its own parent."));
            }

            var parent = await readRepository.GetByIdAsync(request.ParentId.Value, cancellationToken).ConfigureAwait(false);
            if (parent == null)
            {
                return Result<ProductCategoryResponse?>.Failure(Error.NotFound($"Parent category with Id {request.ParentId.Value} not found."));
            }
            if (parent.ParentId.HasValue)
            {
                return Result<ProductCategoryResponse?>.Failure(Error.Validation("Cannot move this category under another child category. Only 2 levels are allowed."));
            }

            var hasSubCategories = await readRepository.HasSubCategoriesAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (hasSubCategories)
            {
                return Result<ProductCategoryResponse?>.Failure(Error.Validation("This category has subcategories and cannot be made a child of another category."));
            }
        }

        category.ParentId = request.ParentId;
        category.MaxPurchaseQuantity = request.MaxPurchaseQuantity;
        updateRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return category.Adapt<ProductCategoryResponse>();
    }
}
