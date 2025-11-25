using Application.ApiContracts.Product;
using Application.ApiContracts.Product.Common;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Enums; // Để dùng DataFetchMode
using Domain.Shared;
using MediatR;

namespace Application.Features.Products.Queries.GetDeletedVariantLiteList;

public sealed class GetDeletedVariantLiteListQueryHandler(IProductVariantReadRepository repository)
    : IRequestHandler<GetDeletedVariantLiteListQuery, PagedResult<ProductVariantLiteResponse>>
{
    public async Task<PagedResult<ProductVariantLiteResponse>> Handle(GetDeletedVariantLiteListQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Max(request.PageSize, 1);

        var (variants, totalCount) = await repository.GetPagedVariantsAsync(
            page,
            pageSize,
            cancellationToken,
            DataFetchMode.DeletedOnly);

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

        return new PagedResult<ProductVariantLiteResponse>(responses, totalCount, page, pageSize);
    }
}