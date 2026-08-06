namespace Application.Features.ChatTools.Queries.GetWarrantyClaimDetailForChat;

public class ChatWarrantyClaimDetailDto
{
    public int ClaimId { get; init; }

    public string ClaimNumber { get; init; } = string.Empty;

    public string? VehicleInfo { get; init; }

    public string? VehiclePlate { get; init; }

    public string? CustomerName { get; init; }

    public string? CustomerPhone { get; init; }

    public string? CustomerAddress { get; init; }

    public string IssueDescription { get; init; } = string.Empty;

    public string? MediaUrls { get; init; }

    public string? ServiceCenterName { get; init; }

    public string? ManufacturerClaimNumber { get; init; }

    public int StatusId { get; init; }

    public string StatusLabel { get; init; } = string.Empty;

    public string? ManufacturerDecision { get; init; }

    public bool IsRecall { get; init; }

    public decimal TotalPartsCost { get; init; }

    public decimal TotalLaborCost { get; init; }

    public IReadOnlyList<ChatWarrantyClaimPartDto> Parts { get; init; } = [];

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}

public class ChatWarrantyClaimPartDto
{
    public string PartName { get; init; } = string.Empty;

    public string? PartCode { get; init; }

    public decimal UnitPrice { get; init; }

    public int Status { get; init; }
}
