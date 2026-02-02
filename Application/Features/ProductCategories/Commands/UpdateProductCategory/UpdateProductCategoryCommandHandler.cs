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
        // Lấy bản ghi (mặc định ActiveOnly)
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        // PC_019 & NotFound: Nếu không thấy hoặc đã bị xóa
        if (category == null || category.DeletedAt != null)
        {
            return Result<ProductCategoryResponse?>.Failure(
                Error.NotFound($"Product category with Id {request.Id} not found or has been deleted."));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var nameToUpdate = request.Name.Trim();

            // PC_017: Check trùng tên với thằng khác
            var isDuplicate = await readRepository.ExistsByNameExceptIdAsync(
                nameToUpdate,
                request.Id,
                cancellationToken,
                DataFetchMode.All).ConfigureAwait(false);

            if (isDuplicate)
            {
                return Result<ProductCategoryResponse?>.Failure(
                    Error.Conflict($"Category name '{nameToUpdate}' already exists."));
            }

            category.Name = nameToUpdate;
        }

        if (request.Description != null)
        {
            category.Description = request.Description.Trim();
        }

        updateRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return category.Adapt<ProductCategoryResponse>();
    }
}
