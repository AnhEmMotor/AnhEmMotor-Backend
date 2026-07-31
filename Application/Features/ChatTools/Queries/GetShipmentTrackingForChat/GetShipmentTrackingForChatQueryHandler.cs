using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Logistics.Shipment;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetShipmentTrackingForChat;

public class GetShipmentTrackingForChatQueryHandler(
    IShipmentReadRepository shipmentReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetShipmentTrackingForChatQuery, Result<ChatToolEnvelope<ChatShipmentTrackingDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatShipmentTrackingDto>>> Handle(
        GetShipmentTrackingForChatQuery request,
        CancellationToken cancellationToken)
    {
        var shipment = await shipmentReadRepository.GetByOutputIdAsync(request.OrderId, cancellationToken)
            .ConfigureAwait(false);
        if (shipment == null)
        {
            return Result<ChatToolEnvelope<ChatShipmentTrackingDto>>.Failure(
                Error.NotFound("Không tìm thấy thông tin vận chuyển"));
        }
        var dto = new ChatShipmentTrackingDto
        {
            OrderId = shipment.OutputId ?? shipment.Id,
            TrackingNumber = shipment.TrackingNumber,
            Carrier = shipment.Carrier,
            Status = shipment.Status.ToString(),
            CreatedAt = shipment.CreatedAt,
            DeliveredAt = shipment.DeliveredAt
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IShipmentReadRepository.GetByOutputIdAsync",
            new Dictionary<string, string>(),
            "van-chuyen",
            null);
        return ChatToolEnvelope<ChatShipmentTrackingDto>.WrapSingle(dto, meta);
    }
}
