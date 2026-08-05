using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListNewsForChat;

public sealed record ListNewsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatNewsListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
