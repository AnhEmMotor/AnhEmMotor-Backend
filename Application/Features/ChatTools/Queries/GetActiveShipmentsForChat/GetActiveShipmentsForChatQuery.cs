using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetActiveShipmentsForChat;

public sealed record GetActiveShipmentsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatActiveShipmentListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
