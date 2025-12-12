using Application.ApiContracts.Product.Responses;
using Application.Interfaces.Repositories.Product;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsList;

public sealed class GetProductsListQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductsListQuery, Domain.Primitives.PagedResult<ProductDetailResponse>>
{
    public async Task<Domain.Primitives.PagedResult<ProductDetailResponse>> Handle(
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

        var items = entities.Select(e => e.Adapt<ProductDetailResponse>()).ToList();

        return new Domain.Primitives.PagedResult<ProductDetailResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);
    }
}