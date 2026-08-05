using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListContactsForChat;

public sealed record ListContactsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatContactListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
