using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSupplierPricesForVariantForChat;

public sealed record GetSupplierPricesForVariantForChatQuery : IRequest<Result<ChatToolEnvelope<ChatSupplierPriceListItemDto>>>
{
    public int VariantId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
