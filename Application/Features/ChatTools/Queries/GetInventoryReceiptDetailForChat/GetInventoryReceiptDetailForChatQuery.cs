using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetInventoryReceiptDetailForChat;

public sealed record GetInventoryReceiptDetailForChatQuery : IRequest<Result<ChatToolEnvelope<ChatInventoryReceiptDetailDto>>>
{
    public int ReceiptId { get; init; }
}
