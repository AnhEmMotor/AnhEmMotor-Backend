using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.Product;
using Domain.Primitives;

using MediatR;

namespace Application.Features.Products.Queries.GetProductsListForManager;

public class GetProductsListForManagerQueryHandler(
    IProductReadRepository readRepository,
    IFileReadService fileReadService) : IRequestHandler<GetProductsListForManagerQuery, Result<PagedResult<ProductDetailForManagerResponse>>>
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
        foreach (var item in allItems)
        {
            if (!string.IsNullOrWhiteSpace(item.CoverImageUrl) &&
                !item.CoverImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                item.CoverImageUrl = fileReadService.GetPublicUrl(item.CoverImageUrl);
            }
            foreach (var variant in item.Variants)
            {
                if (!string.IsNullOrWhiteSpace(variant.CoverImageUrl) &&
                    !variant.CoverImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    variant.CoverImageUrl = fileReadService.GetPublicUrl(variant.CoverImageUrl);
                }
                for (int i = 0; i < variant.PhotoCollection.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(variant.PhotoCollection[i]) &&
                        !variant.PhotoCollection[i].StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        variant.PhotoCollection[i] = fileReadService.GetPublicUrl(variant.PhotoCollection[i]);
                    }
                }
                foreach (var color in variant.Colors)
                {
                    if (!string.IsNullOrWhiteSpace(color.CoverImageUrl) &&
                        !color.CoverImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        color.CoverImageUrl = fileReadService.GetPublicUrl(color.CoverImageUrl);
                    }
                }
            }
        }
        var sortedItems = allItems;
        return new PagedResult<ProductDetailForManagerResponse>(sortedItems, totalCount, request.Page, request.PageSize);
    }
}
