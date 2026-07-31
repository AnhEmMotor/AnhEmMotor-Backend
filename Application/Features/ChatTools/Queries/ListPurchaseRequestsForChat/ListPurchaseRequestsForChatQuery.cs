using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListPurchaseRequestsForChat;

public sealed record ListPurchaseRequestsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatPurchaseRequestListItemDto>>>
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
