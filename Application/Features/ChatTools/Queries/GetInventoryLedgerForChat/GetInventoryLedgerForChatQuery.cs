using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetInventoryLedgerForChat;

public sealed record GetInventoryLedgerForChatQuery : IRequest<Result<ChatToolEnvelope<ChatInventoryLedgerItemDto>>>
{
    public int? ProductId { get; init; }
    public int? VariantId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public int Limit { get; init; } = ChatToolLimit.Default;
}
