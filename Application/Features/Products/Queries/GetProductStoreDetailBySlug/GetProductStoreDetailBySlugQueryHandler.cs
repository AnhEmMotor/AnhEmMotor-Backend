using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Domain.Entities;
using MediatR;
using System.Reflection;

namespace Application.Features.Products.Queries.GetProductStoreDetailBySlug;

public sealed class GetProductStoreDetailBySlugQueryHandler(IProductReadRepository productReadRepository) 
    : IRequestHandler<GetProductStoreDetailBySlugQuery, Result<ProductStoreDetailResponse>>
{
    public async Task<Result<ProductStoreDetailResponse>> Handle(GetProductStoreDetailBySlugQuery request, CancellationToken cancellationToken)
    {
        var variant = await productReadRepository.GetByVariantSlugWithDetailsAsync(request.Slug, cancellationToken);

        if (variant is null || variant.Product is null)
        {
            return Result<ProductStoreDetailResponse>.Failure(new Error("ProductDetail.NotFound", "Không tìm thấy sản phẩm."));
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
            Specifications = []
        };

        var specProperties = typeof(Domain.Entities.Product)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != "Id" && p.Name != "Name" && p.Name != "Description" && 
                        p.Name != "ShortDescription" && p.Name != "MetaTitle" && p.Name != "MetaDescription" &&
                        p.Name != "Brand" && p.Name != "ProductCategory" && p.Name != "ProductStatus" &&
                        p.Name != "ProductVariants" && p.Name != "BrandId" && p.Name != "CategoryId" && 
                        p.Name != "StatusId" && p.Name != "CreatedAt" && p.Name != "UpdatedAt" && 
                        p.Name != "DeletedAt" && p.Name != "CreatedBy" && p.Name != "UpdatedBy");

        foreach (var prop in specProperties)
        {
            var value = prop.GetValue(product);
            if (value is not null)
            {
                if (value is decimal d && d == 0) continue;
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

        var currentVariantResponse = new CurrentVariantStoreResponse
        {
            Id = variant.Id,
            DisplayName = string.Join(" - ", variant.VariantOptionValues
                .Where(vov => vov.OptionValue != null)
                .Select(vov => vov.OptionValue!.Name)),
            Price = variant.Price,
            CoverImageUrl = currentCoverImage,
            PhotoCollection = currentPhotos
        };

        var otherVariants = product.ProductVariants
            .Where(v => v.Id != variant.Id)
            .Select(v => new OtherVariantStoreResponse
            {
                DisplayName = string.Join(" - ", v.VariantOptionValues
                    .Where(vov => vov.OptionValue != null)
                    .Select(vov => vov.OptionValue!.Name)),
                Slug = v.UrlSlug,
                Price = v.Price
            })
            .ToList();

        return Result<ProductStoreDetailResponse>.Success(new ProductStoreDetailResponse
        {
            Product = productResponse,
            CurrentVariant = currentVariantResponse,
            OtherVariants = otherVariants
        });
    }
}
