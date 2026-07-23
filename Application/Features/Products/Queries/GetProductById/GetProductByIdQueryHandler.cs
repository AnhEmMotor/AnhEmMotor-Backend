using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductQuotations;
using Domain.Entities;
using Mapster;
using MediatR;

using System.Linq;

namespace Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler(
    IProductReadRepository readRepository,
    IFileReadService fileReadService,
    IProductQuotationReadRepository? ProductQuotationReadRepository = null) : IRequestHandler<GetProductByIdQuery, Result<ProductDetailForManagerResponse?>>
{
    public async Task<Result<ProductDetailForManagerResponse?>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return Error.NotFound($"Product with Id {request.Id} not found.");
        }
        var response = product.Adapt<ProductDetailForManagerResponse>();
        if (response != null)
        {
            if (!string.IsNullOrWhiteSpace(response.CoverImageUrl) &&
                !response.CoverImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                response.CoverImageUrl = fileReadService.GetPublicUrl(response.CoverImageUrl);
            }
            if (response.Variants != null)
            {
                foreach (var variant in response.Variants)
                {
                    if (!string.IsNullOrWhiteSpace(variant.CoverImageUrl) &&
                        !variant.CoverImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        variant.CoverImageUrl = fileReadService.GetPublicUrl(variant.CoverImageUrl);
                    }
                    if (variant.PhotoCollection != null)
                    {
                        for (int i = 0; i < variant.PhotoCollection.Count; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(variant.PhotoCollection[i]) &&
                                !variant.PhotoCollection[i].StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            {
                                variant.PhotoCollection[i] = fileReadService.GetPublicUrl(variant.PhotoCollection[i]);
                            }
                        }
                    }
                    if (variant.Colors != null)
                    {
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
            }
            await PopulateSupplierPricesAsync(response, product, cancellationToken).ConfigureAwait(false);
        }
        return response;
    }

    private async Task PopulateSupplierPricesAsync(
        ProductDetailForManagerResponse response,
        Product product,
        CancellationToken cancellationToken)
    {
        if (ProductQuotationReadRepository is null)
        {
            return;
        }
        if (response.Variants is null || response.Variants.Count == 0)
        {
            return;
        }
        foreach (var responseVariant in response.Variants)
        {
            var variantEntity = product.ProductVariants.FirstOrDefault(v => v.Id == responseVariant.Id);
            if (variantEntity is null || variantEntity.Id <= 0)
            {
                continue;
            }
            var rows = await ProductQuotationReadRepository
                .GetByVariantAsync(variantEntity.Id, cancellationToken)
                .ConfigureAwait(false);
            responseVariant.SupplierPrices = rows
                .Where(row => row.ProductVariantColorId == null)
                .Select(
                    row => new VariantSupplierPriceRequest
                    {
                        SupplierId = row.SupplierId ?? 0,
                        ProductVariantColorId = row.ProductVariantColorId,
                        QuotePrice = row.QuotePrice,
                        Note = row.Note
                    })
                .ToList();
            var responseColors = responseVariant.Colors ?? [];
            foreach (var responseColor in responseColors)
            {
                var colorId = responseColor.Id;
                if (colorId <= 0)
                {
                    continue;
                }
                responseColor.SupplierPrices = rows
                    .Where(row => row.ProductVariantColorId == colorId)
                    .Select(
                        row => new VariantSupplierPriceRequest
                        {
                            SupplierId = row.SupplierId ?? 0,
                            ProductVariantColorId = row.ProductVariantColorId,
                            QuotePrice = row.QuotePrice,
                            Note = row.Note
                        })
                    .ToList();
            }
        }
    }
}

