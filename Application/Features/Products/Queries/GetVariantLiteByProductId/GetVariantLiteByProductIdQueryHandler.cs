using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Enums;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantLiteByProductId;

public sealed class GetVariantLiteByProductIdQueryHandler(
    IProductReadRepository productReadRepository,
    IProductVariantReadRepository variantReadRepository)
    : IRequestHandler<GetVariantLiteByProductIdQuery, (List<ProductVariantLiteResponse>? Data, ErrorResponse? Error)>
{
    public async Task<(List<ProductVariantLiteResponse>? Data, ErrorResponse? Error)> Handle(GetVariantLiteByProductIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Check Product exists (Cheap check)
        // Chỉ dùng hàm GetByIdAsync đơn giản, không load quan hệ, tiết kiệm bộ nhớ
        var product = await productReadRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Product with Id {request.ProductId} not found." }]
            });
        }

        // 2. Determine Fetch Mode
        var mode = request.IncludeDeleted ? DataFetchMode.All : DataFetchMode.ActiveOnly;

        // 3. Fetch Variants via Repository
        var variants = await variantReadRepository.GetByProductIdAsync(request.ProductId, cancellationToken, mode);

        // 4. Map to Response
        var responses = variants.Select(v =>
        {
            var optionPairs = v.VariantOptionValues
                .Select(vov => new OptionPair
                {
                    OptionName = vov.OptionValue?.Option?.Name,
                    OptionValue = vov.OptionValue?.Name
                })
                .ToList();

            var stock = v.InputInfos?.Sum(ii => ii.RemainingCount) ?? 0;

            return ProductResponseMapper.BuildVariantLiteResponse(
                v.Id,
                v.Product?.Id,
                v.Product?.Name,
                optionPairs,
                v.Price,
                v.CoverImageUrl,
                stock
            );
        }).ToList();

        return (responses, null);
    }
}