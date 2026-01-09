using Application.ApiContracts.ProductCategory.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreProductCategory;

public sealed class RestoreProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreProductCategoryCommand, (ProductCategoryResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(ProductCategoryResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        RestoreProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(category == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail
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
