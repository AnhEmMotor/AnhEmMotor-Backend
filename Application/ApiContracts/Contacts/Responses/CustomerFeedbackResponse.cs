namespace Application.ApiContracts.Contacts.Responses;

public record CustomerFeedbackResponse
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int Rating { get; set; }
    public string FeedbackArea { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public ContactBasicResponse? Contact { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}
