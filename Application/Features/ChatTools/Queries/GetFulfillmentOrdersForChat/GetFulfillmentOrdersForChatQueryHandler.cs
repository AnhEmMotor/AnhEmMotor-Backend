using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Enums;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetFulfillmentOrdersForChat;

public class GetFulfillmentOrdersForChatQueryHandler(
    IShipmentReadRepository shipmentReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetFulfillmentOrdersForChatQuery, Result<ChatToolEnvelope<ChatFulfillmentOrderListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatFulfillmentOrderListItemDto>>> Handle(
        GetFulfillmentOrdersForChatQuery request,
        CancellationToken cancellationToken)
    {
        var shipments = await shipmentReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var query = shipments.AsEnumerable();
        var filtersApplied = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ParcelDeliveryStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(x => x.Status == status);
            filtersApplied["Trạng thái"] = status.ToString();
        }

        if (!string.IsNullOrWhiteSpace(request.Carrier))
        {
            query = query.Where(x => string.Equals(x.Carrier, request.Carrier, StringComparison.OrdinalIgnoreCase));
            filtersApplied["Đơn vị vận chuyển"] = request.Carrier;
        }

        if (request.FromDate.HasValue)
        {
            var fromUtc = new DateTimeOffset(
                DateTime.SpecifyKind(dateProvider.VietnamDayRangeUtc(request.FromDate.Value).StartUtc, DateTimeKind.Utc));
            query = query.Where(x => x.CreatedAt >= fromUtc);
        }

        if (request.ToDate.HasValue)
        {
            var toUtc = new DateTimeOffset(
                DateTime.SpecifyKind(dateProvider.VietnamDayRangeUtc(request.ToDate.Value).EndUtc, DateTimeKind.Utc));
            query = query.Where(x => x.CreatedAt <= toUtc);
        }

        if (request.FromDate.HasValue || request.ToDate.HasValue)
        {
            filtersApplied["Khoảng thời gian"] = $"{request.FromDate:yyyy-MM-dd} đến {request.ToDate:yyyy-MM-dd}";
        }

        var ordered = query.OrderByDescending(x => x.CreatedAt).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);

        var dtos = ordered
            .Take(limit)
            .Select(
                x => new ChatFulfillmentOrderListItemDto
                {
                    Id = x.Id,
                    TrackingNumber = x.TrackingNumber ?? string.Empty,
                    OriginalOrderCode = x.OutputId?.ToString() ?? string.Empty,
                    CustomerName = x.CustomerName ?? string.Empty,
                    Carrier = x.Carrier ?? string.Empty,
                    Status = x.Status.ToString(),
                    CodAmount = x.CodAmount,
                    CreatedAt = x.CreatedAt,
                    DeliveredAt = x.DeliveredAt
                })
            .ToList();

        var inner = new ChatToolResult<ChatFulfillmentOrderListItemDto>(dtos, ordered.Count, ordered.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IShipmentReadRepository.GetAllAsync",
            filtersApplied,
            "don-can-xu-ly",
            "VND");

        return ChatToolEnvelope<ChatFulfillmentOrderListItemDto>.Wrap(inner, meta);
    }
}
