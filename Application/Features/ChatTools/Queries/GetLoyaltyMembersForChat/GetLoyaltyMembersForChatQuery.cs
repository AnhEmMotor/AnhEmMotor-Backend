using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLoyaltyMembersForChat;

public sealed record GetLoyaltyMembersForChatQuery : IRequest<Result<ChatToolEnvelope<ChatLoyaltyMemberDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
