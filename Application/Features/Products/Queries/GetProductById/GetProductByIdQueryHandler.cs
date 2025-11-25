using Application.ApiContracts.Product;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductByIdQuery, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(
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

        var response = product.Adapt<ProductDetailResponse>();
        return (response, null);
    }
}
