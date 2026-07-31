using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetRevenueByCategoryForChat;

public sealed record GetRevenueByCategoryForChatQuery : IRequest<Result<ChatToolEnvelope<ChatRevenueByCategoryItemDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
