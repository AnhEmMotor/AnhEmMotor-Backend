using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreProductCategory;

public sealed class RestoreProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreProductCategoryCommand, Result<ProductCategoryResponse?>>
{
    public async Task<Result<ProductCategoryResponse?>> Handle(
        RestoreProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(category == null)
        {
            return Error.NotFound($"Product category with Id {request.Id} not found in deleted categories.");
        }

        updateRepository.Restore(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return category.Adapt<ProductCategoryResponse>();
    }
}
