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

    public string? AssignedUserName { get; set; }

    public DateTimeOffset? AssignedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public int? EmployeeRatingOfCustomer { get; set; }

    public string? EmployeeRatingComment { get; set; }

    public DateTimeOffset? EmployeeRatedAt { get; set; }

    public int? CustomerRatingOfEmployee { get; set; }

    public string? CustomerRatingComment { get; set; }

    public DateTimeOffset? CustomerRatedAt { get; set; }

    public ContactBasicResponse? Contact { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
}
