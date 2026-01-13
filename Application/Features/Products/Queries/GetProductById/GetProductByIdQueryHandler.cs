using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;

using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductByIdQuery, Result<ProductDetailForManagerResponse?>>
{
    public async Task<Result<ProductDetailForManagerResponse?>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(product == null)
        {
            return Error.NotFound($"Product with Id {request.Id} not found.");
        }

        var response = product.Adapt<ProductDetailForManagerResponse>();
        return response;
    }
}
