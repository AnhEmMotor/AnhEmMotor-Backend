using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetFulfillmentOrdersForChat;

public sealed record GetFulfillmentOrdersForChatQuery : IRequest<Result<ChatToolEnvelope<ChatFulfillmentOrderListItemDto>>>
{
    /// <summary>
    /// "shipping" | "completed" | "returned" — để trống nếu không lọc theo trạng thái.
    /// </summary>
    public string? Status { get; init; }

    public string? Carrier { get; init; }

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
