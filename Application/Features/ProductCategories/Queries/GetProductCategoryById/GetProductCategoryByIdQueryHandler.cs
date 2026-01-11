using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;

using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Queries.GetProductCategoryById;

public sealed class GetProductCategoryByIdQueryHandler(IProductCategoryReadRepository repository) : IRequestHandler<GetProductCategoryByIdQuery, Result<ProductCategoryResponse?>>
{
    public async Task<Result<ProductCategoryResponse?>> Handle(
        GetProductCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(category == null)
        {
            return Error.NotFound($"Product category with Id {request.Id} not found.");
        }

        return category.Adapt<ProductCategoryResponse>();
    }
}
