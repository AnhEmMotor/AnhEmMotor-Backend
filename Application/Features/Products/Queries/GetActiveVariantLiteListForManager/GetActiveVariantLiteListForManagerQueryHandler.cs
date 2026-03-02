using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.PredefinedOption;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Primitives;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetActiveVariantLiteListForManager;

public sealed class GetActiveVariantLiteListForManagerQueryHandler(
    IProductVariantReadRepository repository,
    IPredefinedOptionReadRepository predefinedOptionReadRepository) : IRequestHandler<GetActiveVariantLiteListForManagerQuery, Result<PagedResult<ProductVariantLiteResponse>>>
{
    public async Task<Result<PagedResult<ProductVariantLiteResponse>>> Handle(
        GetActiveVariantLiteListForManagerQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Max(request.PageSize, 1);

        var translations = await predefinedOptionReadRepository
            .GetAllAsDictionaryAsync(cancellationToken)
            .ConfigureAwait(false);

        var (variants, totalCount) = await repository.GetPagedVariantsAsync(
            page,
            pageSize,
            request.Filters,
            request.Sorts,
            cancellationToken,
            search: request.Search)
            .ConfigureAwait(false);

        var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponse>()).ToList();

        return new PagedResult<ProductVariantLiteResponse>(responses, totalCount, page, pageSize);
    }
}