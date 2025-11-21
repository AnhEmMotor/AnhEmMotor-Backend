using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreProductCategory;

public sealed class RestoreProductCategoryCommandHandler(
    IProductCategorySelectRepository selectRepository,
    IProductCategoryRestoreRepository restoreRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreProductCategoryCommand, (ProductCategoryResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> Handle(RestoreProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var deletedCategories = await selectRepository.GetDeletedCategoriesByIdsAsync([request.Id], cancellationToken).ConfigureAwait(false);
        
        if (deletedCategories.Count == 0)
        {
            var category = await selectRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return (null, new ErrorResponse
                {
                    Errors = [new ErrorDetail { Message = $"Danh mục sản phẩm với Id {request.Id} không tồn tại." }]
                });
            }
            
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Danh mục sản phẩm với Id {request.Id} chưa bị xoá." }]
            });
        }

        var categoryToRestore = deletedCategories[0];
        restoreRepository.Restore(categoryToRestore);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = new ProductCategoryResponse
        {
            Id = categoryToRestore.Id,
            Name = categoryToRestore.Name,
            Description = categoryToRestore.Description
        };

        return (response, null);
    }
}
