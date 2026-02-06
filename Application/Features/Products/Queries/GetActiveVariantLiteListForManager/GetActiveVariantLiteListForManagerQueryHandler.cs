using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Primitives;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetActiveVariantLiteListForManager;

public sealed class GetActiveVariantLiteListForManagerQueryHandler(IProductVariantReadRepository repository) : IRequestHandler<GetActiveVariantLiteListForManagerQuery, Result<PagedResult<ProductVariantLiteResponse>>>
{
    public async Task<Result<PagedResult<ProductVariantLiteResponse>>> Handle(
        GetActiveVariantLiteListForManagerQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Max(request.PageSize, 1);

        var (variants, totalCount) = await repository.GetPagedVariantsAsync(
            page,
            pageSize,
            request.Filters,
            request.Sorts,
            cancellationToken)
            .ConfigureAwait(false);

        var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponse>()).ToList();

        return new PagedResult<ProductVariantLiteResponse>(responses, totalCount, page, pageSize);
    }
}