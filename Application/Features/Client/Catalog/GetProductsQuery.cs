using Application.ApiContracts.Client.Catalog;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.Product;
using ProductStatus = Domain.Constants.Product.ProductStatus;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Client.Catalog;

public record GetProductsQuery(string? Search, int? CategoryId) : IRequest<List<ProductSummaryResponse>>;

public record GetProductDetailQuery(int Id) : IRequest<ProductDetailResponse>;

public record RequestConsultationCommand(ConsultationRequest Request) : IRequest<bool>;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<ProductSummaryResponse>>
{
    private readonly IProductReadRepository _readRepo;
    private readonly IFileReadService _fileReadService;

    public GetProductsHandler(IProductReadRepository readRepo, IFileReadService fileReadService)
    {
        _readRepo = readRepo;
        _fileReadService = fileReadService;
    }

    public async Task<List<ProductSummaryResponse>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var statusIds = new List<string> { ProductStatus.ForSale };
        string? normalizedSearch = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

        var (entities, totalCount, groupedOptionFilters) = await _readRepo.GetPagedProductsAsync(
            normalizedSearch, statusIds,
            request.CategoryId.HasValue ? new List<int> { request.CategoryId.Value } : new List<int>(),
            new List<int>(), new List<int>(), null, null,
            1, 50, null, null, cancellationToken)
            .ConfigureAwait(false);

        var effectiveBrandName = entities.FirstOrDefault()?.ProductVariants.FirstOrDefault()?.Product?.Brand?.Name ?? "AnhEm Motor";

        var items = entities.Select(e =>
        {
            var brandName = e.Brand?.Name ?? effectiveBrandName;
            var catName = e.ProductCategory?.Name ?? "";

            var sortedVariants = e.ProductVariants.ToList();
            sortedVariants.Sort((a, b) => (a.Price ?? decimal.MaxValue).CompareTo(b.Price ?? decimal.MaxValue));

            var firstVariant = sortedVariants
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.VariantName) || v.ProductVariantColors.Count > 0)
                ?? sortedVariants.FirstOrDefault();

            string ResolveImgUrl(ProductVariant? v)
            {
                if (v == null) return string.Empty;
                var cover = v.ProductVariantColors
                    .Where(c => !string.IsNullOrEmpty(c.CoverImageUrl) && !c.CoverImageUrl.Contains("dummyimage"))
                    .Select(c => c.CoverImageUrl!)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(cover)) return _fileReadService.GetPublicUrl(cover);
                cover = v.CoverImageUrl;
                if (!string.IsNullOrEmpty(cover) && !cover.Contains("dummyimage")) return _fileReadService.GetPublicUrl(cover);
                var photo = v.ProductCollectionPhotos
                    .Where(p => !string.IsNullOrEmpty(p.ImageUrl) && !p.ImageUrl.Contains("dummyimage"))
                    .Select(p => p.ImageUrl!)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(photo)) return _fileReadService.GetPublicUrl(photo);
                return string.Empty;
            }

            var coverUrl = ResolveImgUrl(firstVariant);

            decimal? minPrice = e.ProductVariants
                .Where(v => v.Price.HasValue)
                .Min(v => v.Price);

            List<string> features = e.ProductTechnologies
                ?.OrderBy(t => t.DisplayOrder)
                .Select(t => t.CustomDescription ?? t.Technology?.DefaultDescription ?? t.CustomTitle ?? "")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Cast<string>()
                .ToList() ?? new List<string>();

            string promotionText = BuildPromotionText(e, firstVariant);

            return new ProductSummaryResponse
            {
                Id = e.Id,
                Name = e.Name,
                ImageUrl = coverUrl,
                ReferencePrice = minPrice ?? 0,
                PromotionText = promotionText
            };
        }).ToList();

        return items;
    }

    private static string BuildPromotionText(Product product, ProductVariant? firstVariant)
    {
        var parts = new List<string> { "Chính hãng" };
        if (product.ProductTechnologies != null && product.ProductTechnologies.Any())
        {
            var techNames = product.ProductTechnologies
                .Where(t => t.DisplayOrder <= 3)
                .Select(t => t.CustomTitle ?? t.Technology?.DefaultTitle ?? "")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(2)
                .Cast<string>();
            parts.AddRange(techNames);
        }
        if (firstVariant != null && firstVariant.Price.HasValue)
        {
            var maxQty = firstVariant.MaxPurchaseQuantity;
            if (maxQty.HasValue) parts.Add($"Tối đa {maxQty} chiếc/đơn");
        }
        return string.Join(" · ", parts);
    }
}

public class GetProductDetailHandler : IRequestHandler<GetProductDetailQuery, ProductDetailResponse>
{
    private readonly IProductReadRepository _readRepo;
    private readonly IFileReadService _fileReadService;

    public GetProductDetailHandler(IProductReadRepository readRepo, IFileReadService fileReadService)
    {
        _readRepo = readRepo;
        _fileReadService = fileReadService;
    }

    public async Task<ProductDetailResponse> Handle(
        GetProductDetailQuery request,
        CancellationToken cancellationToken)
    {
        var variant = await _readRepo.GetVariantByIdWithDetailsAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (variant is null || variant.Product is null)
        {
            return new ProductDetailResponse { Id = request.Id, Name = "Không tìm thấy", Description = "", ReferencePrice = 0, Features = new List<string>(), IsCompatibleWithMyVehicle = false, CompatibilityNote = "" };
        }

        var product = variant.Product;
        string ResolveImgUrl(ProductVariant v)
        {
            var cover = v.ProductVariantColors
                .Where(c => !string.IsNullOrEmpty(c.CoverImageUrl) && !c.CoverImageUrl.Contains("dummyimage"))
                .Select(c => c.CoverImageUrl!)
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(cover)) return _fileReadService.GetPublicUrl(cover);
            cover = v.CoverImageUrl;
            if (!string.IsNullOrEmpty(cover) && !cover.Contains("dummyimage")) return _fileReadService.GetPublicUrl(cover);
            var photo = v.ProductCollectionPhotos
                .Where(p => !string.IsNullOrEmpty(p.ImageUrl) && !p.ImageUrl.Contains("dummyimage"))
                .Select(p => p.ImageUrl!)
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(photo)) return _fileReadService.GetPublicUrl(photo);
            return string.Empty;
        }

        string mainImage = ResolveImgUrl(variant);
        var techList = (product.ProductTechnologies ?? new List<ProductTechnology>())
            .OrderBy(t => t.DisplayOrder)
            .Select(t =>
            {
                var title = t.CustomTitle ?? t.Technology?.DefaultTitle ?? "";
                var desc = t.CustomDescription ?? t.Technology?.DefaultDescription ?? "";
                return string.IsNullOrWhiteSpace(desc) ? title : $"{title}: {desc}";
            })
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Take(8)
            .Cast<string>()
            .ToList();

        bool IsCompatible = product.CategoryId == 8;

        return new ProductDetailResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description ?? "",
            ReferencePrice = variant.Price ?? 0,
            Features = techList,
            IsCompatibleWithMyVehicle = IsCompatible,
            CompatibilityNote = IsCompatible ? "Liên hệ để kiểm tra tương thích với xe của bạn" : ""
        };
    }
}

public class RequestConsultationHandler : IRequestHandler<RequestConsultationCommand, bool>
{
    private readonly ILeadInsertRepository _leadRepo;

    public RequestConsultationHandler(ILeadInsertRepository leadRepo) => _leadRepo = leadRepo;

    public async Task<bool> Handle(RequestConsultationCommand request,
        CancellationToken cancellationToken)
    {
        var lead = new Lead
        {
            InterestedVehicle = request.Request.ProductId.ToString() ?? "",
            Notes = request.Request.CustomerNote,
            Source = "Catalog",
            CreatedAt = DateTime.UtcNow,
            Status = "New",
            Priority = "Warm"
        };
        await _leadRepo.AddAsync(lead, cancellationToken);
        return true;
    }
}
