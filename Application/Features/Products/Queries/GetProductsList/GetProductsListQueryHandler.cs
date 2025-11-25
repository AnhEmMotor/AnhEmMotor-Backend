using Application.Interfaces.Repositories.Product;
using Domain.Shared;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsList;

public sealed class GetProductsListQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductsListQuery, PagedResult<ApiContracts.Product.Responses.ProductDetailResponse>>
{
    public async Task<PagedResult<ApiContracts.Product.Responses.ProductDetailResponse>> Handle(
        GetProductsListQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedStatusIds = request.StatusIds
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var (entities, totalCount) = await readRepository.GetPagedProductsAsync(
            request.Search,
            normalizedStatusIds,
            request.Page,
            request.PageSize,
            cancellationToken)
            .ConfigureAwait(false);

        var items = entities.Select(e => e.Adapt<ApiContracts.Product.Responses.ProductDetailResponse>()).ToList();

        return new PagedResult<ApiContracts.Product.Responses.ProductDetailResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);
    }
}