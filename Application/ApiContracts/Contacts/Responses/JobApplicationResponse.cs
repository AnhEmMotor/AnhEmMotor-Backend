namespace Application.ApiContracts.Contacts.Responses;

public record JobApplicationResponse
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AppliedPosition { get; set; } = string.Empty;
    public string CvFileUrl { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public string Status { get; set; } = string.Empty;
    public ContactBasicResponse? Contact { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}
