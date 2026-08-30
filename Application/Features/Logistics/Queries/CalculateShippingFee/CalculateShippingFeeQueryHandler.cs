using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Services.Shipping;
using Application.Interfaces.Services.Shipping.Models;
using MediatR;

namespace Application.Features.Logistics.Queries.CalculateShippingFee;

public class CalculateShippingFeeQueryHandler(
    IShippingService shippingService,
    IProductVariantReadRepository variantReadRepository,
    IProductReadRepository productReadRepository) : IRequestHandler<CalculateShippingFeeQuery, Result<decimal>>
{
    public async Task<Result<decimal>> Handle(CalculateShippingFeeQuery request, CancellationToken cancellationToken)
    {
        var enrichedItems = new List<ShippingItemDto>();
        foreach (var item in request.Items)
        {
            var variant = await variantReadRepository.GetByIdWithDetailsAsync(item.ProductVariantId, cancellationToken);
            if (variant == null)
                continue;
            var product = await productReadRepository.GetByIdWithDetailsAsync(variant.ProductId, cancellationToken);
            if (product == null)
                continue;
            var weight = variant.Weight > 0 ? variant.Weight : (product.Weight > 0 ? product.Weight : 1000);
            var length = variant.Length > 0 ? variant.Length : (product.Length > 0 ? product.Length : 10);
            var width = variant.Width > 0 ? variant.Width : (product.Width > 0 ? product.Width : 10);
            var height = variant.Height > 0 ? variant.Height : (product.Height > 0 ? product.Height : 10);
            enrichedItems.Add(
                new ShippingItemDto
                {
                    Name = variant.VariantName ?? product.Name ?? string.Empty,
                    Quantity = item.Quantity,
                    Weight = (int)(weight! * 1000),
                    Length = (int)length!,
                    Width = (int)width!,
                    Height = (int)height!
                });
        }
        var calculateRequest = new CalculateShippingFeeRequest
        {
            ToWardIdV2 = int.TryParse(request.WardId, out var wardId) ? wardId : 0,
            ToAddressV2 = string.Empty,
            IsNewToAddress = true,
            ToWardCode = request.WardId,
            Items = enrichedItems
        };
        return await shippingService.CalculateShippingFeeAsync(calculateRequest, cancellationToken);
    }
}

