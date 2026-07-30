using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLowStockProductsForChat;

public sealed record GetLowStockProductsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatLowStockProductDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
