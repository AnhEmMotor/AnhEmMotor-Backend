using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetShipmentTrackingForChat;

public sealed record GetShipmentTrackingForChatQuery : IRequest<Result<ChatToolEnvelope<ChatShipmentTrackingDto>>>
{
    public int OrderId { get; init; }
}
