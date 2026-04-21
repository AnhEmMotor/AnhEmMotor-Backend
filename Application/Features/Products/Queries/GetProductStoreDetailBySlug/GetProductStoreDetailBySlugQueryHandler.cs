using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Domain.Constants.Product;
using Domain.Entities;
using MediatR;
using System.Reflection;

namespace Application.Features.Products.Queries.GetProductStoreDetailBySlug;

public sealed class GetProductStoreDetailBySlugQueryHandler(IProductReadRepository productReadRepository) : IRequestHandler<GetProductStoreDetailBySlugQuery, Result<ProductStoreDetailResponse>>
{
    public async Task<Result<ProductStoreDetailResponse>> Handle(
        GetProductStoreDetailBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var decodedSlug = System.Net.WebUtility.UrlDecode(request.Slug);
        var variant = await productReadRepository.GetByVariantSlugWithDetailsAsync(decodedSlug, cancellationToken)
            .ConfigureAwait(false);

        if(variant is null || variant.Product is null)
        {
            return Result<ProductStoreDetailResponse>.Failure(
                new Error("ProductDetail.NotFound", "Không tìm thấy sản phẩm."));
        }

        var product = variant.Product;

        var productResponse = new ProductInfoStoreResponse
        {
            Name = product.Name,
            Brand = product.Brand?.Name,
            Category = product.ProductCategory?.Name,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            Highlights = product.ProductTechnologies?.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(
                product.ProductTechnologies.OrderBy(t => t.DisplayOrder).Select(t => new
                {
                    title = t.CustomTitle ?? t.Technology?.DefaultTitle ?? t.Technology?.Name,
                    tag = t.Technology?.Category?.Name ?? "TECHNOLOGY",
                    description = t.CustomDescription ?? t.Technology?.DefaultDescription,
                    image = t.CustomImageUrl ?? t.Technology?.DefaultImageUrl
                })) : null,
            Specifications = []
        };

        var specProperties = typeof(Product)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => !ProductAttributeLabels.IsInternalProperty(p.Name));

        foreach(var prop in specProperties)
        {
            var value = prop.GetValue(product);
            if(value is not null)
            {
                if(value is decimal d && d == 0)
                    continue;
                productResponse.Specifications[prop.Name] = value;
            }
        }

        var currentPhotos = variant.ProductCollectionPhotos
            .Where(p => !string.IsNullOrEmpty(p.ImageUrl))
            .Select(p => p.ImageUrl!)
            .ToList();
        var currentCoverImage = string.IsNullOrWhiteSpace(variant.CoverImageUrl)
            ? currentPhotos.FirstOrDefault()
            : variant.CoverImageUrl;

        var currentDisplayName = !string.IsNullOrWhiteSpace(variant.VersionName) && !string.IsNullOrWhiteSpace(variant.ColorName)
            ? $"{variant.VersionName} - {variant.ColorName}"
            : (!string.IsNullOrWhiteSpace(variant.VersionName) ? variant.VersionName : (variant.ColorName ?? "Tiêu chuẩn"));

        var currentVariantResponse = new CurrentVariantStoreResponse
        {
            Id = variant.Id,
            DisplayName = currentDisplayName,
            UrlSlug = variant.UrlSlug,
            Price = variant.Price,
            CoverImageUrl = currentCoverImage,
            ColorName = variant.ColorName,
            ColorCode = variant.ColorCode,
            PhotoCollection = currentPhotos
        };

        var otherVariants = product.ProductVariants
            .Where(v => v.Id != variant.Id)
            .Select(
                v => {
                    var displayName = !string.IsNullOrWhiteSpace(v.VersionName) && !string.IsNullOrWhiteSpace(v.ColorName)
                        ? $"{v.VersionName} - {v.ColorName}"
                        : (!string.IsNullOrWhiteSpace(v.VersionName) ? v.VersionName : (v.ColorName ?? "Tiêu chuẩn"));

                    return new OtherVariantStoreResponse
                    {
                        DisplayName = displayName,
                        UrlSlug = v.UrlSlug,
                        Price = v.Price,
                        CoverImageUrl = v.CoverImageUrl,
                        ColorName = v.ColorName,
                        ColorCode = v.ColorCode
                    };
                })
            .ToList();

        return Result<ProductStoreDetailResponse>.Success(
            new ProductStoreDetailResponse
            {
                Product = productResponse,
                CurrentVariant = currentVariantResponse,
                OtherVariants = otherVariants
            });
    }
}
