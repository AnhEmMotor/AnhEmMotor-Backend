using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Mapster;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed class CreateProductCategoryCommandHandler(
    IProductCategoryInsertRepository repository,
    IProductCategoryReadRepository readRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCategoryCommand, Result<ProductCategoryResponse>>
{
    public async Task<Result<ProductCategoryResponse>> Handle(
        CreateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var categoryName = request.Name?.Trim();
        var isExisted = await readRepository.ExistsByNameAsync(categoryName!, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);
        if (isExisted)
        {
            return Result<ProductCategoryResponse>.Failure(
                Error.Conflict($"Category name '{categoryName}' already exists."));
        }
        var category = request.Adapt<ProductCategoryEntity>();
        if (request.ParentId.HasValue)
        {
            var parent = await readRepository.GetByIdAsync(request.ParentId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (parent == null)
            {
                return Result<ProductCategoryResponse>.Failure(
                    Error.NotFound($"Parent category with Id {request.ParentId.Value} not found."));
            }
            if (parent.ParentId.HasValue)
            {
                return Result<ProductCategoryResponse>.Failure(
                    Error.Validation(
                        "Cannot create a category at this level. Only 2 levels are allowed (Parent and Child)."));
            }
        }
        category.Name = categoryName;
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        repository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return category.Adapt<ProductCategoryResponse>();
    }
}
