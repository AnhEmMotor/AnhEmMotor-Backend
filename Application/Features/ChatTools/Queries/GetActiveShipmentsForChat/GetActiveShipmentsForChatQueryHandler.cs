using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Enums;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetActiveShipmentsForChat;

public class GetActiveShipmentsForChatQueryHandler(
    IShipmentReadRepository shipmentReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetActiveShipmentsForChatQuery, Result<ChatToolEnvelope<ChatActiveShipmentListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatActiveShipmentListItemDto>>> Handle(
        GetActiveShipmentsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var shipments = await shipmentReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var now = dateProvider.UtcNow;
        var limit = ChatToolLimit.Clamp(request.Limit);
        var active = shipments
            .Where(x => x.Status == ParcelDeliveryStatus.Shipping && !x.DeliveredAt.HasValue)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
        var dtos = active
            .Take(limit)
            .Select(
                x => new ChatActiveShipmentListItemDto
                {
                    Id = x.Id,
                    TrackingNumber = x.TrackingNumber ?? string.Empty,
                    CustomerName = x.CustomerName ?? string.Empty,
                    Carrier = x.Carrier ?? string.Empty,
                    CodAmount = x.CodAmount,
                    CreatedAt = x.CreatedAt,
                    DaysInTransit = (int)(now - (x.CreatedAt ?? now)).TotalDays
                })
            .ToList();
        var inner = new ChatToolResult<ChatActiveShipmentListItemDto>(dtos, active.Count, active.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IShipmentReadRepository.GetAllAsync",
            new Dictionary<string, string>(),
            "van-don-dang-giao",
            "VND");
        return ChatToolEnvelope<ChatActiveShipmentListItemDto>.Wrap(inner, meta);
    }
}
