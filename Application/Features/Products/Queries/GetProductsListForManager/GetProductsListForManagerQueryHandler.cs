using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.Setting;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsListForManager;

public sealed class GetProductsListForManagerQueryHandler(
    IProductReadRepository readRepository,
    ISettingRepository settingRepository) : IRequestHandler<GetProductsListForManagerQuery, Result<PagedResult<ProductDetailForManagerResponse>>>
{
    public async Task<Result<PagedResult<ProductDetailForManagerResponse>>> Handle(
        GetProductsListForManagerQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var alertLevelStr = settings.FirstOrDefault(
            s => string.Equals(s.Key, SettingKeys.InventoryAlertLevel, StringComparison.OrdinalIgnoreCase))?.Value;
        long.TryParse(alertLevelStr, out var alertLevel);
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
            .Select(e => ProductMappingConfig.MapProductToDetailForManagerResponseWithAlertLevel(e, alertLevel))
            .ToList();
        var filtered = string.IsNullOrWhiteSpace(request.InventoryStatusFilter)
            ? allItems
            : [.. allItems.Where(i => string.Compare(i.InventoryStatus, request.InventoryStatusFilter) == 0)];
        var sortedItems = request.SortByInventoryStatus switch
        {
            SortDirection.Ascending => filtered.OrderBy(i => InventoryStatus.GetSeverity(i.InventoryStatus)).ToList(),
            SortDirection.Descending => filtered.OrderByDescending(i => InventoryStatus.GetSeverity(i.InventoryStatus))
                .ToList(),
            _ => filtered
        };
        return new PagedResult<ProductDetailForManagerResponse>(sortedItems, totalCount, request.Page, request.PageSize);
    }
}