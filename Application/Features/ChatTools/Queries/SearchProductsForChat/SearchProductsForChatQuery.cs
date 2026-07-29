using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.SearchProductsForChat;

public sealed record SearchProductsForChatQuery : IRequest<Result<ChatToolResult<ChatProductSearchDto>>>
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
