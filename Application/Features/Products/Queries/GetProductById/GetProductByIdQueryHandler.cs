using Application.Interfaces.Repositories.Product;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductByIdQuery, (ApiContracts.Product.Responses.ProductDetailResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(ApiContracts.Product.Responses.ProductDetailResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(product == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors = [ new Common.Models.ErrorDetail { Message = $"Product with Id {request.Id} not found." } ]
            });
        }

        var response = product.Adapt<ApiContracts.Product.Responses.ProductDetailResponse>();
        return (response, null);
    }
}
