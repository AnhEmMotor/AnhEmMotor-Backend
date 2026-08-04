using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.User;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListUsersAndRolesForChat;

public class ListUsersAndRolesForChatQueryHandler(
    IUserReadRepository userReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<ListUsersAndRolesForChatQuery, Result<ChatToolEnvelope<ChatUserRoleListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatUserRoleListItemDto>>> Handle(
        ListUsersAndRolesForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel { Page = 1, PageSize = limit };
        var paged = await userReadRepository.GetPagedListAsync(sieveModel, cancellationToken).ConfigureAwait(false);
        var dtos = (paged.Items ?? [])
            .Select(
                u => new ChatUserRoleListItemDto
                {
                    UserName = u.UserName ?? string.Empty,
                    FullName = u.FullName ?? string.Empty,
                    Roles = (u.Roles ?? []).ToList()
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatUserRoleListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IUserReadRepository.GetPagedListAsync",
            new Dictionary<string, string>(),
            "nguoi-dung-va-vai-tro",
            null);
        return ChatToolEnvelope<ChatUserRoleListItemDto>.Wrap(inner, meta);
    }
}
