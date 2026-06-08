namespace Application.ApiContracts.Contacts.Responses;

public record SupportRequestResponse
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? OrderCode { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedUserId { get; set; }
    public ContactBasicResponse? Contact { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}
