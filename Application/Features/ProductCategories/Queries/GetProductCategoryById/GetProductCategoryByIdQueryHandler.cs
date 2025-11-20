using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Queries.GetProductCategoryById;

public sealed class GetProductCategoryByIdQueryHandler(IProductCategorySelectRepository repository)
    : IRequestHandler<GetProductCategoryByIdQuery, (ProductCategoryResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> Handle(GetProductCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (category == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Product category with Id {request.Id} not found." }]
            });
        }

        return (new ProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        }, null);
    }
}
