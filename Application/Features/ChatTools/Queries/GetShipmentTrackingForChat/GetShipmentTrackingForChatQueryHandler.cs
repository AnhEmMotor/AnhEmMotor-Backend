using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Application.Interfaces.Repositories.Output;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetShipmentTrackingForChat;

public class GetShipmentTrackingForChatQueryHandler(
    IOutputReadRepository outputReadRepository,
    IShipmentReadRepository shipmentReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetShipmentTrackingForChatQuery, Result<ChatToolEnvelope<ChatShipmentTrackingDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatShipmentTrackingDto>>> Handle(
        GetShipmentTrackingForChatQuery request,
        CancellationToken cancellationToken)
    {
        var keyword = request.Keyword.Trim();
        var orderIds = await ChatToolOrderSearch
            .FindOrderIdsByKeywordAsync(outputReadRepository, keyword, cancellationToken)
            .ConfigureAwait(false);
        var dtos = new List<ChatShipmentTrackingDto>();
        foreach (var orderId in orderIds)
        {
            var shipment = await shipmentReadRepository.GetByOutputIdAsync(orderId, cancellationToken)
                .ConfigureAwait(false);
            if (shipment == null)
            {
                continue;
            }
            dtos.Add(
                new ChatShipmentTrackingDto
                {
                    OrderId = shipment.OutputId ?? shipment.Id,
                    TrackingNumber = shipment.TrackingNumber,
                    Carrier = shipment.Carrier,
                    Status = shipment.Status.ToString(),
                    CreatedAt = shipment.CreatedAt,
                    DeliveredAt = shipment.DeliveredAt
                });
        }
        var inner = new ChatToolResult<ChatShipmentTrackingDto>(dtos, dtos.Count, false);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IOutputReadRepository.GetPagedAsync+IShipmentReadRepository.GetByOutputIdAsync",
            new Dictionary<string, string> { ["Từ khóa"] = keyword },
            "van-chuyen",
            null);
        return ChatToolEnvelope<ChatShipmentTrackingDto>.Wrap(inner, meta);
    }
}
