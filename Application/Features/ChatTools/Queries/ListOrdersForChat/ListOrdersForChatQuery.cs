using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListOrdersForChat;

public sealed record ListOrdersForChatQuery : IRequest<Result<ChatToolEnvelope<ChatOrderListItemDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public string? StatusId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
