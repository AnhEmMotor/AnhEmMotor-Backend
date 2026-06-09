using Application.Common.Models;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Primitives;
using MediatR;
using System.Globalization;

namespace Application.Features.News.Queries.GetProductsForNews;

public class GetProductsForNewsQueryHandler : IRequestHandler<GetProductsForNewsQuery, Result<PagedResult<ProductNewsDto>>>
{
    private readonly IProductVariantReadRepository _productVariantReadRepository;

    public GetProductsForNewsQueryHandler(IProductVariantReadRepository productVariantReadRepository)
    {
        _productVariantReadRepository = productVariantReadRepository;
    }

    public async Task<Result<PagedResult<ProductNewsDto>>> Handle(GetProductsForNewsQuery request, CancellationToken cancellationToken)
    {
        var page = request.SieveModel?.Page ?? 1;
        var pageSize = request.SieveModel?.PageSize ?? 10;

        var (items, totalCount) = await _productVariantReadRepository.GetPagedVariantsAsync(
            page: page,
            pageSize: pageSize,
            filters: request.SieveModel?.Filters,
            sorts: request.SieveModel?.Sorts,
            cancellationToken: cancellationToken);

        var resultItems = new List<ProductNewsDto>();

        foreach (var variant in items)
        {
            var baseName = variant.Product?.Name ?? "";
            var variantName = string.IsNullOrWhiteSpace(variant.VariantName) ? baseName : $"{baseName} - {variant.VariantName}";
            var price = variant.Price?.ToString("C0", CultureInfo.GetCultureInfo("vi-VN")) ?? "Liên hệ";
            var baseImage = variant.CoverImageUrl ?? "";

            var dto = new ProductNewsDto
            {
                Id = variant.Id.ToString(),
                Name = variantName,
                Price = price,
                Img = baseImage
            };

            if (variant.ProductVariantColors.Any())
            {
                foreach (var color in variant.ProductVariantColors)
                {
                    dto.Colors.Add(new ProductNewsColorDto
                    {
                        Id = color.Id.ToString(),
                        Name = color.ColorName ?? "",
                        Img = color.CoverImageUrl ?? baseImage
                    });
                }
            }

            resultItems.Add(dto);
        }

        var pagedResult = new PagedResult<ProductNewsDto>(resultItems, totalCount, page, pageSize);
        return Result<PagedResult<ProductNewsDto>>.Success(pagedResult);
    }
}
