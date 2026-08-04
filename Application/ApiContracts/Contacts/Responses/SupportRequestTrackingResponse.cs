using Domain.Enums;

namespace Application.ApiContracts.Contacts.Responses;

public record SupportRequestTrackingResponse
{
    public int Id { get; init; }

    public string Subject { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string? AssignedUserName { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? AssignedAt { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? ClosedAt { get; init; }

    public int? CustomerRatingOfEmployee { get; init; }

    public string? CustomerRatingComment { get; init; }

    public bool CanCustomerRate => Status == SupportRequestStatus.Closed && AssignedUserName is not null;
}
