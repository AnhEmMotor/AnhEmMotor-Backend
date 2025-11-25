using Application.ApiContracts.Product.Common;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Enums;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantLiteByProductId;

public sealed class GetVariantLiteByProductIdQueryHandler(
    IProductReadRepository productReadRepository,
    IProductVariantReadRepository variantReadRepository) : IRequestHandler<GetVariantLiteByProductIdQuery, (List<ProductVariantLiteResponse>? Data, ErrorResponse? Error)>
{
    public async Task<(List<ProductVariantLiteResponse>? Data, ErrorResponse? Error)> Handle(
        GetVariantLiteByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productReadRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if(product == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = $"Product with Id {request.ProductId} not found." } ]
            });
        }

        var mode = request.IncludeDeleted ? DataFetchMode.All : DataFetchMode.ActiveOnly;

        var variants = await variantReadRepository.GetByProductIdAsync(request.ProductId, cancellationToken, mode);

        var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponse>()).ToList();

        return (responses, null);
    }
}