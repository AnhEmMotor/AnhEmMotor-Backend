using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.Product;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsListForManager;

public class GetProductsListForManagerQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductsListForManagerQuery, Result<PagedResult<ProductDetailForManagerResponse>>>
{
    public async Task<Result<PagedResult<ProductDetailForManagerResponse>>> Handle(
        GetProductsListForManagerQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedStatusIds = request.StatusIds
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var (entities, totalCount, _) = await readRepository.GetPagedProductsAsync(
            request.Search,
            normalizedStatusIds,
            [],
            [],
            [],
            null,
            null,
            request.Page,
            request.PageSize,
            request.Filters,
            request.Sorts,
            cancellationToken)
            .ConfigureAwait(false);
        var allItems = entities
            .Select(ProductMappingConfig.MapProductToDetailForManagerResponseWithAlertLevel)
            .ToList();
        var sortedItems = allItems;
        return new PagedResult<ProductDetailForManagerResponse>(sortedItems, totalCount, request.Page, request.PageSize);
    }
}
