using Application.Common.Models;
using Application.Interfaces.Repositories.ProductVariant;
using MediatR;
using System.Globalization;

namespace Application.Features.News.Queries.GetProductsForNews;

public class GetProductsForNewsQueryHandler : IRequestHandler<GetProductsForNewsQuery, Result<List<ProductNewsDto>>>
{
    private readonly IProductVariantReadRepository _productVariantReadRepository;

    public GetProductsForNewsQueryHandler(IProductVariantReadRepository productVariantReadRepository)
    {
        _productVariantReadRepository = productVariantReadRepository;
    }

    public async Task<Result<List<ProductNewsDto>>> Handle(GetProductsForNewsQuery request, CancellationToken cancellationToken)
    {
        var (items, _) = await _productVariantReadRepository.GetPagedVariantsAsync(
            page: 1,
            pageSize: 1000,
            filters: null,
            sorts: null,
            cancellationToken: cancellationToken);

        var result = new List<ProductNewsDto>();

        foreach (var variant in items)
        {
            var baseName = variant.Product?.Name ?? "";
            var variantName = string.IsNullOrWhiteSpace(variant.VariantName) ? baseName : $"{baseName} - {variant.VariantName}";
            var price = variant.Price?.ToString("C0", CultureInfo.GetCultureInfo("vi-VN")) ?? "Liên hệ";
            var baseImage = variant.CoverImageUrl ?? "";

            if (variant.ProductVariantColors.Any())
            {
                foreach (var color in variant.ProductVariantColors)
                {
                    result.Add(new ProductNewsDto
                    {
                        Id = $"{variant.Id}_{color.Id}",
                        Name = $"{variantName} - {color.ColorName}",
                        Price = price,
                        Img = color.CoverImageUrl ?? baseImage
                    });
                }
            }
            else
            {
                result.Add(new ProductNewsDto
                {
                    Id = variant.Id.ToString(),
                    Name = variantName,
                    Price = price,
                    Img = baseImage
                });
            }
        }

        return Result<List<ProductNewsDto>>.Success(result);
    }
}
