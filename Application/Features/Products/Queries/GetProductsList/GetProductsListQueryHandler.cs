using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.Product;
using Domain.Constants.Product;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsList;

public sealed class GetProductsListQueryHandler(IProductReadRepository readRepository) : IRequestHandler<GetProductsListQuery, Result<PagedResult<ProductListStoreResponse>>>
{
    public async Task<Result<PagedResult<ProductListStoreResponse>>> Handle(
        GetProductsListQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedStatusIds = request.StatusIds
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (normalizedStatusIds.Count == 0)
        {
            normalizedStatusIds.Add(ProductStatus.ForSale);
        }
        var (entities, totalCount, groupedOptionFilters) = await readRepository.GetPagedProductsAsync(
            request.Search,
            normalizedStatusIds,
            request.CategoryIds,
            request.BrandIds,
            request.OptionValueIds,
            request.MinPrice,
            request.MaxPrice,
            request.Page,
            request.PageSize,
            request.Filters,
            request.Sorts,
            cancellationToken)
            .ConfigureAwait(false);
        var normalizedSearch = request.Search?.Trim();
        var items = entities.Select(
            e =>
            {
                var isExactVariantSearch = !string.IsNullOrWhiteSpace(normalizedSearch) &&
                    e.ProductVariants.Any(
                        v => string.Equals(
                                v.VariantName?.Trim(),
                                normalizedSearch,
                                StringComparison.OrdinalIgnoreCase) ||
                            v.VariantOptionValues.Any(
                                vov => string.Equals(
                                    vov.OptionValue?.Name?.Trim(),
                                    normalizedSearch,
                                    StringComparison.OrdinalIgnoreCase)));
                var productMetadataMatches = string.IsNullOrWhiteSpace(normalizedSearch) ||
                    (!string.IsNullOrWhiteSpace(e.Name) &&
                        e.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.ProductCategory?.Name) &&
                        e.ProductCategory.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.Brand?.Name) &&
                        e.Brand.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));

                bool MatchesSearch(Domain.Entities.ProductVariant variant)
                {
                    var exactVariantMatches =
                        string.Equals(
                            variant.VariantName?.Trim(),
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase) ||
                        variant.VariantOptionValues.Any(
                            vov => string.Equals(
                                vov.OptionValue?.Name?.Trim(),
                                normalizedSearch,
                                StringComparison.OrdinalIgnoreCase));
                    if (isExactVariantSearch)
                    {
                        return exactVariantMatches;
                    }

                    return productMetadataMatches ||
                        (!string.IsNullOrWhiteSpace(variant.VariantName) &&
                            variant.VariantName.Contains(
                                normalizedSearch!,
                                StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(variant.SKU) &&
                            variant.SKU.Contains(normalizedSearch!, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(variant.UrlSlug) &&
                            variant.UrlSlug.Contains(normalizedSearch!, StringComparison.OrdinalIgnoreCase)) ||
                        variant.ProductVariantColors.Any(
                            c => !string.IsNullOrWhiteSpace(c.ColorName) &&
                                c.ColorName.Contains(
                                    normalizedSearch!,
                                    StringComparison.OrdinalIgnoreCase)) ||
                        variant.VariantOptionValues.Any(
                            vov => !string.IsNullOrWhiteSpace(vov.OptionValue?.Name) &&
                                vov.OptionValue.Name.Contains(
                                    normalizedSearch!,
                                    StringComparison.OrdinalIgnoreCase));
                }

                return new ProductListStoreResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    Category = e.ProductCategory?.Name,
                    Brand = e.Brand?.Name,
                    Displacement = e.Displacement,
                    HasBeenBooked = e.Id % 20,
                    Variants =
                        [.. e.ProductVariants
                            .Where(
                                v => MatchesSearch(v) &&
                                    (request.OptionValueIds.Count == 0 ||
                                                (groupedOptionFilters?.Count > 0 &&
                                                    groupedOptionFilters.All(
                                                        group => v.VariantOptionValues
                                                                .Any(
                                                                    vov => vov.OptionValueId != null &&
                                                                                                        group.Ids
                                                                                                            .Contains(
                                                                                                                vov.OptionValueId.Value)) ||
                                                            v.ProductVariantColors
                                                                .Any(
                                                                    c => c.ColorName != null &&
                                                                                                        group.Names
                                                                                                            .Contains(
                                                                                                                c.ColorName
                                                                                                                                                            .Trim(
                                                                                                                                                                )
                                                                                                                                                            .ToLower(
                                                                                                                                                                ))) ||
                                                            (v.VariantName != null &&
                                                                v.VariantName
                                                                    .Split(',')
                                                                    .Any(
                                                                        vn => group.Names.Contains(vn.Trim().ToLower())))))) &&
                                            (!request.MinPrice.HasValue || v.Price >= request.MinPrice.Value) &&
                                            (!request.MaxPrice.HasValue || v.Price <= request.MaxPrice.Value))
                            .Select(
                                v =>
                                {
                                    var allColors = ProductMappingConfig.MapVariantColors(v);
                                    var matchingColors = productMetadataMatches || string.IsNullOrWhiteSpace(normalizedSearch)
                                        ? []
                                        : allColors
                                            .Where(
                                                c => !string.IsNullOrWhiteSpace(c.ColorName) &&
                                                    c.ColorName.Contains(
                                                        normalizedSearch,
                                                        StringComparison.OrdinalIgnoreCase))
                                            .ToList();
                                    var responseColors = matchingColors.Count > 0 ? matchingColors : allColors;
                                    var photos = v.ProductCollectionPhotos
                                        .Where(p => !string.IsNullOrEmpty(p.ImageUrl))
                                        .Select(p => p.ImageUrl!)
                                        .ToList();
                                    var preferredColorId = matchingColors.FirstOrDefault()?.Id;
                                    var variantColor = preferredColorId.HasValue
                                        ? v.ProductVariantColors.FirstOrDefault(c => c.Id == preferredColorId.Value)
                                        : v.ProductVariantColors.FirstOrDefault();
                                    var colorName = variantColor?.ColorName;
                                    var coverImageUrl = !string.IsNullOrWhiteSpace(variantColor?.CoverImageUrl)
                                        ? variantColor.CoverImageUrl
                                        : v.CoverImageUrl;
                                    var coverImage = string.IsNullOrWhiteSpace(coverImageUrl)
                                        ? photos.FirstOrDefault()
                                        : coverImageUrl;
                                    var variantDisplayName = !string.IsNullOrWhiteSpace(v.VariantName) &&
                                                        !string.IsNullOrWhiteSpace(colorName)
                                        ? $"{v.VariantName} - {colorName}"
                                        : (!string.IsNullOrWhiteSpace(v.VariantName)
                                                        ? v.VariantName
                                                        : (colorName ?? "Tiêu chuẩn"));
                                    return new ProductVariantListStoreResponse
                            {
                                Id = v.Id,
                                UrlSlug = v.UrlSlug,
                                Price = v.Price,
                                CoverImageUrl = coverImage,
                                OptionValuesText = variantDisplayName,
                                Colors = responseColors,
                                ProductLimit = ProductMappingConfig.GetEffectiveMaxPurchaseQuantity(v, null),
                                EffectiveMax = ProductMappingConfig.GetEffectiveMaxPurchaseQuantity(v, null)
                            };
                                })
                            .OrderBy(v => v.Price)
                            .ToList()]
                };
            })
            .ToList();
        return new PagedResult<ProductListStoreResponse>(items, totalCount, request.Page, request.PageSize);
    }
}
