using Application.ApiContracts.Product.Common;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Shared;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetActiveVariantLiteList;

public sealed class GetActiveVariantLiteListQueryHandler(IProductVariantReadRepository repository) : IRequestHandler<GetActiveVariantLiteListQuery, PagedResult<ProductVariantLiteResponse>>
{
    public async Task<PagedResult<ProductVariantLiteResponse>> Handle(
        GetActiveVariantLiteListQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Max(request.PageSize, 1);

        var (variants, totalCount) = await repository.GetPagedVariantsAsync(page, pageSize, cancellationToken);

        var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponse>()).ToList();

        return new PagedResult<ProductVariantLiteResponse>(responses, totalCount, page, pageSize);
    }
}