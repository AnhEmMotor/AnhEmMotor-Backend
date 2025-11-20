using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductSelectRepository selectRepository)
    : IRequestHandler<GetProductByIdQuery, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await selectRepository
            .GetProductWithDetailsByIdAsync(request.Id, request.IncludeDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (product == null)
        {
            return (null, new ErrorResponse
            {
                Errors =
                [
                    new ErrorDetail
                    {
                        Message = $"Product with Id {request.Id} not found."
                    }
                ]
            });
        }

        var response = ProductResponseMapper.BuildProductDetailResponse(product);
        return (response, null);
    }
}
