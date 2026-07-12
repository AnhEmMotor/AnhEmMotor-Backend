using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using Domain.Enums;
using MediatR;
using System.Linq;

namespace Application.Features.Logistics.Queries.GetFulfillmentDetail;

public class GetFulfillmentDetailQueryHandler : IRequestHandler<GetFulfillmentDetailQuery, FulfillmentDetailResponse>
{
    private readonly IShipmentReadRepository _context;
    private readonly IParcelDeliveryOrderReadRepository _parcelOrderRepo;

    public GetFulfillmentDetailQueryHandler(
        IShipmentReadRepository context,
        IParcelDeliveryOrderReadRepository parcelOrderRepo)
    {
        _context = context;
        _parcelOrderRepo = parcelOrderRepo;
    }

    public async Task<FulfillmentDetailResponse> Handle(
        GetFulfillmentDetailQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _context.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (order == null)
            return null!;
        var parcelOrder = await _parcelOrderRepo.FindByTrackingOrPhoneAsync(order.TrackingNumber, cancellationToken);
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
                        i =>
                        {
                            var pItem = parcelOrder?.Items?.FirstOrDefault(
                                pi => pi.ProductId == (i.ProductVariant?.ProductId ?? 0) || pi.Id == i.Id);
                            return new FulfillmentDetailItemResponse
                    {
                        Id = pItem?.Id ?? i.Id,
                        ProductId = i.ProductVariant?.ProductId ?? 0,
                        ProductName = GenerateProductName(i),
                        ThumbnailUrl =
                            i.ProductVariantColor?.CoverImageUrl ?? i.ProductVariant?.CoverImageUrl ?? string.Empty,
                        ShelfLocation = pItem?.ShelfLocation ?? "A1-01",
                        Quantity = i.Quantity,
                        IsPicked = pItem?.IsPicked ?? false,
                        IsRestricted = pItem?.IsRestricted ?? false,
                        IsOutOfStock = pItem?.IsOutOfStock ?? false
                    };
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
