namespace Application.Features.ChatTools.Queries.GetLeadDetailForChat;

public record ChatLeadDetailDto
{
    public int LeadId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public string InterestedVehicle { get; init; } = string.Empty;

    public int Score { get; init; }

    public string Tier { get; init; } = string.Empty;

    public int Points { get; init; }

    public string? AssignedToName { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
