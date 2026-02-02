using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services;

using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed class DeleteProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryDeleteRepository deleteRepository,
    IProtectedProductCategoryService protectedCategoryService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        // Mặc định lấy ActiveOnly
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        // SỬA TẠI ĐÂY: Kiểm tra cả null và trạng thái đã xóa
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

        deleteRepository.Delete(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
