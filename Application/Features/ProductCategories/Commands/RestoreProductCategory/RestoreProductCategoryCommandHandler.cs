using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreProductCategory;

public sealed class RestoreProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreProductCategoryCommand, (ProductCategoryResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> Handle(
        RestoreProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(category == null)
        {
            return (null, new ErrorResponse
            {
                Errors =
                    [ new ErrorDetail
                    {
                        Message = $"Product category with Id {request.Id} not found in deleted categories."
                    } ]
            });
        }

        updateRepository.Restore(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (category.Adapt<ProductCategoryResponse>(), null);
    }
}
