using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetTopSellingForChat;

public sealed record GetTopSellingForChatQuery : IRequest<Result<ChatToolResult<ChatTopSellingProductDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
