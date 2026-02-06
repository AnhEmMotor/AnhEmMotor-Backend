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

        if(isExisted)
        {
            return Result<ProductCategoryResponse>.Failure(
                Error.Conflict($"Category name '{categoryName}' already exists."));
        }

        var category = request.Adapt<ProductCategoryEntity>();
        category.Name = categoryName;
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        repository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return category.Adapt<ProductCategoryResponse>();
    }
}