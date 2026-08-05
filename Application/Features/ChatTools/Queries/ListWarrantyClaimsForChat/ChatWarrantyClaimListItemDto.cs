namespace Application.Features.ChatTools.Queries.ListWarrantyClaimsForChat;

public record ChatWarrantyClaimListItemDto
{
    public int ClaimId { get; init; }

    public string? VehicleInfo { get; init; }

    public string? CustomerName { get; init; }

    public int StatusId { get; init; }

    public string StatusLabel { get; init; } = string.Empty;

    public DateTimeOffset? CreatedAt { get; init; }
}
