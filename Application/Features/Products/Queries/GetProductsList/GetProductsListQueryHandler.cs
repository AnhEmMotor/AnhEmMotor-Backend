using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
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
        var items = entities.Select(
            e => new ProductListStoreResponse
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
                                v => (request.OptionValueIds.Count == 0 ||
                                                (groupedOptionFilters?.Count > 0 &&
                                                    groupedOptionFilters.All(
                                                        group => v.VariantOptionValues
                                                                .Any(
                                                                    vov => vov.OptionValueId != null &&
                                                                                                        group.Ids
                                                                                                            .Contains(
                                                                                                                vov.OptionValueId.Value)) ||
                                                            (v.ColorName != null &&
                                                                v.ColorName
                                                                    .Split(',')
                                                                    .Any(
                                                                        cn => group.Names.Contains(cn.Trim().ToLower()))) ||
                                                            (v.VersionName != null &&
                                                                v.VersionName
                                                                    .Split(',')
                                                                    .Any(
                                                                        vn => group.Names.Contains(vn.Trim().ToLower())))))) &&
                                            (!request.MinPrice.HasValue || v.Price >= request.MinPrice.Value) &&
                                            (!request.MaxPrice.HasValue || v.Price <= request.MaxPrice.Value))
                            .Select(
                                v =>
                                {
                                    var photos = v.ProductCollectionPhotos
                                        .Where(p => !string.IsNullOrEmpty(p.ImageUrl))
                                        .Select(p => p.ImageUrl!)
                                        .ToList();
                                    var coverImage = string.IsNullOrWhiteSpace(v.CoverImageUrl)
                                        ? photos.FirstOrDefault()
                                        : v.CoverImageUrl;
                                    var variantDisplayName = !string.IsNullOrWhiteSpace(v.VersionName) &&
                                                        !string.IsNullOrWhiteSpace(v.ColorName)
                                        ? $"{v.VersionName} - {v.ColorName}"
                                        : (!string.IsNullOrWhiteSpace(v.VersionName)
                                                        ? v.VersionName
                                                        : (v.ColorName ?? "Tiêu chuẩn"));
                                    return new ProductVariantListStoreResponse
                            {
                                Id = v.Id,
                                UrlSlug = v.UrlSlug,
                                Price = v.Price,
                                CoverImageUrl = coverImage,
                                OptionValuesText = variantDisplayName
                            };
                                })
                            .OrderBy(v => v.Price)
                            .ToList()]
            })
            .ToList();
        return new PagedResult<ProductListStoreResponse>(items, totalCount, request.Page, request.PageSize);
    }
}