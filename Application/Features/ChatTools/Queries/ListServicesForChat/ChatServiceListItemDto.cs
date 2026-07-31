namespace Application.Features.ChatTools.Queries.ListServicesForChat;

public sealed record ChatServiceListItemDto
{
    public int ServiceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal BasePrice { get; init; }
    public int? EstimatedDurationMinutes { get; init; }
    public bool IsActive { get; init; }
}
