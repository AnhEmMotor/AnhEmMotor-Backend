namespace Application.ApiContracts.Contacts.Responses;

public record ContactReplyResponse
{
    public int Id { get; set; }

    public int ContactId { get; set; }

    public string Message { get; set; } = string.Empty;

    public Guid? RepliedById { get; set; }

    public string? RepliedByName { get; set; }

    public bool IsInternal { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
}
