using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetProductPriceListForChat;

public sealed record GetProductPriceListForChatQuery : IRequest<Result<ChatToolEnvelope<ChatProductPriceListItemDto>>>
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
