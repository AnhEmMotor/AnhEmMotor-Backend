using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductByIdQuery, (ApiContracts.Product.Responses.ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ApiContracts.Product.Responses.ProductDetailResponse? Data, ErrorResponse? Error)> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(product == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = $"Product with Id {request.Id} not found." } ]
            });
        }

        var response = product.Adapt<ApiContracts.Product.Responses.ProductDetailResponse>();
        return (response, null);
    }
}
