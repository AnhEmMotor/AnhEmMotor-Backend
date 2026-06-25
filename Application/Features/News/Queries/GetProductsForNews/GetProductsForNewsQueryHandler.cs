using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Primitives;
using MediatR;
using System.Globalization;

namespace Application.Features.News.Queries.GetProductsForNews;

public class GetProductsForNewsQueryHandler(IProductVariantReadRepository productVariantReadRepository) : IRequestHandler<GetProductsForNewsQuery, Result<PagedResult<ProductNewsResponse>>>
{
    public async Task<Result<PagedResult<ProductNewsResponse>>> Handle(
        GetProductsForNewsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.SieveModel?.Page ?? 1;
        var pageSize = request.SieveModel?.PageSize ?? 10;
        var (items, totalCount) = await productVariantReadRepository.GetPagedVariantsAsync(
            page: page,
            pageSize: pageSize,
            filters: request.SieveModel?.Filters,
            sorts: request.SieveModel?.Sorts,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var resultItems = new List<ProductNewsResponse>();
        if (items is not null)
        {
            foreach (var variant in items)
            {
                var baseName = variant.Product?.Name ?? string.Empty;
                var variantName = string.IsNullOrWhiteSpace(variant.VariantName)
                    ? baseName
                    : $"{baseName} - {variant.VariantName}";
                var price = variant.Price?.ToString("C0", CultureInfo.GetCultureInfo("vi-VN")) ?? "Liên hệ";
                var baseImage = variant.CoverImageUrl ?? string.Empty;
                var dto = new ProductNewsResponse
                {
                    Id = variant.Id.ToString(),
                    Name = variantName,
                    Price = price,
                    Img = baseImage
                };
                if (variant.ProductVariantColors.Count != 0)
                {
                    foreach (var color in variant.ProductVariantColors)
                    {
                        dto.Colors
                            .Add(
                                new ProductNewsColorResponse
                                {
                                    Id = color.Id.ToString(),
                                    Name = color.ColorName ?? string.Empty,
                                    Img = color.CoverImageUrl ?? baseImage
                                });
                    }
                }
                resultItems.Add(dto);
            }
        }
        var pagedResult = new PagedResult<ProductNewsResponse>(resultItems, totalCount, page, pageSize);
        return Result<PagedResult<ProductNewsResponse>>.Success(pagedResult);
    }
}
