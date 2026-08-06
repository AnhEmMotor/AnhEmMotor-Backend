namespace Application.Features.ChatTools.Queries.GetLeadPipelineForChat;

public record ChatLeadPipelineItemDto
{
    public int LeadId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string StatusDisplayName { get; init; } = string.Empty;

    public int Score { get; init; }

    public string InterestedVehicle { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }
}
