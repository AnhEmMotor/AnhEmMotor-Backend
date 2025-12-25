using Application.ApiContracts.Product.Responses;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Primitives;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetActiveVariantLiteListForManager;

public sealed class GetActiveVariantLiteListForManagerQueryHandler(IProductVariantReadRepository repository) : IRequestHandler<GetActiveVariantLiteListForManagerQuery, PagedResult<ProductVariantLiteResponse>>
{
    public async Task<PagedResult<ProductVariantLiteResponse>> Handle(
        GetActiveVariantLiteListForManagerQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Max(request.PageSize, 1);

        var (variants, totalCount) = await repository.GetPagedVariantsAsync(page, pageSize, cancellationToken)
            .ConfigureAwait(false);

        var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponse>()).ToList();

        return new PagedResult<ProductVariantLiteResponse>(responses, totalCount, page, pageSize);
    }
}