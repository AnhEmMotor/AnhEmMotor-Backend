using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
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

        var (entities, totalCount, groupedOptionValueIds) = await readRepository.GetPagedProductsAsync(
            request.Search,
            normalizedStatusIds,
            request.CategoryIds,
            request.OptionValueIds,
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
                Variants =
                    [ .. e.ProductVariants
                            .Where(
                                v => request.OptionValueIds.Count == 0 ||
                                            (groupedOptionValueIds?.Count > 0 &&
                                                groupedOptionValueIds.All(
                                                    groupIds => v.VariantOptionValues
                                                        .Any(
                                                            vov => vov.OptionValueId != null &&
                                                                                        groupIds.Contains(
                                                                                            vov.OptionValueId.Value)))))
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

                                    return new ProductVariantListStoreResponse
                            {
                                Id = v.Id,
                                UrlSlug = v.UrlSlug,
                                Price = v.Price,
                                CoverImageUrl = coverImage,
                                OptionValuesText =
                                    string.Join(
                                                    " - ",
                                                    v.VariantOptionValues
                                                        .Where(vov => vov.OptionValue != null)
                                                        .Select(vov => vov.OptionValue!.Name))
                            };
                                }) ]
            })
            .ToList();

        return new PagedResult<ProductListStoreResponse>(items, totalCount, request.Page, request.PageSize);
    }
}