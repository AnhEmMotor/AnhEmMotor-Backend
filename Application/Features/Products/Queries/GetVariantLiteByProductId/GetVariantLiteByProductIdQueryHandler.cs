using Application.ApiContracts.Product.Responses;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductVariant;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantLiteByProductId;

public sealed class GetVariantLiteByProductIdQueryHandler(
    IProductReadRepository productReadRepository,
    IProductVariantReadRepository variantReadRepository) : IRequestHandler<GetVariantLiteByProductIdQuery, (List<ProductVariantLiteResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(List<ProductVariantLiteResponse>? Data, Common.Models.ErrorResponse? Error)> Handle(
        GetVariantLiteByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productReadRepository.GetByIdAsync(request.ProductId, cancellationToken)
            .ConfigureAwait(false);

        if(product == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail { Message = $"Product with Id {request.ProductId} not found." } ]
            });
        }

        var mode = request.IncludeDeleted ? DataFetchMode.All : DataFetchMode.ActiveOnly;

        var variants = await variantReadRepository.GetByProductIdAsync(request.ProductId, cancellationToken, mode)
            .ConfigureAwait(false);

        var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponse>()).ToList();

        return (responses, null);
    }
}