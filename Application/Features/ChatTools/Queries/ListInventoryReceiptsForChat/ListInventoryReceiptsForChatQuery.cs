using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListInventoryReceiptsForChat;

public sealed record ListInventoryReceiptsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatInventoryReceiptListItemDto>>>
{
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public int Limit { get; init; } = ChatToolLimit.Default;
}
