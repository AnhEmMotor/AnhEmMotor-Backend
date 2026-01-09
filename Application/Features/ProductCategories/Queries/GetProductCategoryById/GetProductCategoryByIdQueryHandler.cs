using Application.ApiContracts.ProductCategory.Responses;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Common.Models;
using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Queries.GetProductCategoryById;

public sealed class GetProductCategoryByIdQueryHandler(IProductCategoryReadRepository repository) : IRequestHandler<GetProductCategoryByIdQuery, (ProductCategoryResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(ProductCategoryResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        GetProductCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(category == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail { Message = $"Product category with Id {request.Id} not found." } ]
            });
        }

        return (category.Adapt<ProductCategoryResponse>(), null);
    }
}
