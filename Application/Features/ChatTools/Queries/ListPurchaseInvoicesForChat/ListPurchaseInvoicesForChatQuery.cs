using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListPurchaseInvoicesForChat;

public sealed record ListPurchaseInvoicesForChatQuery : IRequest<Result<ChatToolEnvelope<ChatPurchaseInvoiceListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
