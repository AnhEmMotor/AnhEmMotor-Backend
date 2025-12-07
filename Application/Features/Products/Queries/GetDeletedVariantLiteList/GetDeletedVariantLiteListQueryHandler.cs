using Application.ApiContracts.Product.Common;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetDeletedVariantLiteList;

public sealed class GetDeletedVariantLiteListQueryHandler(IProductVariantReadRepository repository) : IRequestHandler<GetDeletedVariantLiteListQuery, Domain.Primitives.PagedResult<ProductVariantLiteResponse>>
{
    public async Task<Domain.Primitives.PagedResult<ProductVariantLiteResponse>> Handle(
        GetDeletedVariantLiteListQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Max(request.PageSize, 1);

        var (variants, totalCount) = await repository.GetPagedVariantsAsync(
            page,
            pageSize,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponse>()).ToList();

        return new Domain.Primitives.PagedResult<ProductVariantLiteResponse>(responses, totalCount, page, pageSize);
    }
}