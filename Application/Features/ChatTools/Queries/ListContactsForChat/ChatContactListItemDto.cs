namespace Application.Features.ChatTools.Queries.ListContactsForChat;

public record ChatContactListItemDto
{
    public int ContactId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Subject { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset? CreatedAt { get; init; }
}
