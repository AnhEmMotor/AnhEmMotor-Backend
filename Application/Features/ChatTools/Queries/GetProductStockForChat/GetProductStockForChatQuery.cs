using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetProductStockForChat;

public sealed record GetProductStockForChatQuery : IRequest<Result<ChatToolResult<ChatProductStockDto>>>
{
    public int ProductId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
