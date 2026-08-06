using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListUsersAndRolesForChat;

public sealed record ListUsersAndRolesForChatQuery : IRequest<Result<ChatToolEnvelope<ChatUserRoleListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
