using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetPurchaseRequestDetailForChat;

public sealed record GetPurchaseRequestDetailForChatQuery : IRequest<Result<ChatToolEnvelope<ChatPurchaseRequestDetailDto>>>
{
    public int PurchaseRequestId { get; init; }
}
