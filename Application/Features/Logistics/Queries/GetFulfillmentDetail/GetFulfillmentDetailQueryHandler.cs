using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Entities.Logistics;
using Domain.Enums;
using MediatR;
using System.Linq;

namespace Application.Features.Logistics.Queries.GetFulfillmentDetail;

public class GetFulfillmentDetailQueryHandler : IRequestHandler<GetFulfillmentDetailQuery, FulfillmentDetailResponse>
{
    private readonly IShipmentReadRepository _context;

    public GetFulfillmentDetailQueryHandler(IShipmentReadRepository context)
    {
        _context = context;
    }

    public async Task<FulfillmentDetailResponse> Handle(
        GetFulfillmentDetailQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _context.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (order == null)
            return null!;
        return new FulfillmentDetailResponse
        {
            Id = order.Id,
            TrackingNumber = order.TrackingNumber,
            OriginalOrderCode = order.OutputId?.ToString() ?? string.Empty,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerAddress = order.DestinationAddress,
            Carrier = order.Carrier,
            Status =
                order.Status == ParcelDeliveryStatus.Shipping && order.DeliveredAt.HasValue
                    ? ParcelDeliveryStatus.Completed
                    : order.Status,
            CodAmount = order.CodAmount,
            ShippingCost = order.ShippingCost,
            CreatedAt = order.CreatedAt.HasValue ? order.CreatedAt.Value.UtcDateTime : default,
            ExpectedAt = null,
            DeliveredAt = order.DeliveredAt.HasValue ? order.DeliveredAt.Value.UtcDateTime : (DateTime?)null,
            Items =
                order.Items
                    .Select(
                        i => new FulfillmentDetailItemResponse
                    {
                        Id = i.Id,
                        ProductId = i.ProductVariant?.ProductId ?? 0,
                        ProductName = GenerateProductName(i),
                        ThumbnailUrl =
                            i.ProductVariantColor?.CoverImageUrl ?? i.ProductVariant?.CoverImageUrl ?? string.Empty,
                        ShelfLocation = string.Empty,
                        Quantity = i.Quantity,
                        IsPicked = true,
                        IsRestricted = false,
                        IsOutOfStock = false
                    })
                    .ToList()
        };
    }

    private static string GenerateProductName(ShipmentItem item)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(item.ProductVariant?.Product?.Name))
            parts.Add(item.ProductVariant.Product.Name);
        if (!string.IsNullOrWhiteSpace(item.ProductVariant?.VariantName))
            parts.Add(item.ProductVariant.VariantName);
        if (!string.IsNullOrWhiteSpace(item.ProductVariantColor?.ColorName))
            parts.Add(item.ProductVariantColor.ColorName);
        return string.Join(" - ", parts);
    }
}
