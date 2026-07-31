namespace Application.Features.ChatTools.Queries.ListUsersAndRolesForChat;

public record ChatUserRoleListItemDto
{
    public string UserName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public IReadOnlyList<string> Roles { get; init; } = [];
}
