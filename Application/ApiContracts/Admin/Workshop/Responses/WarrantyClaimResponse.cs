namespace Application.ApiContracts.Admin.Workshop.Responses;

public class WarrantyClaimResponse
{
    public int Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string? VehicleInfo { get; set; }
    public string? VehiclePlate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string? StatusText { get; set; }
    public string? VehicleVin { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public string? MediaUrls { get; set; }
    public string? ServiceCenterName { get; set; }
    public string? ManufacturerClaimNumber { get; set; }
    public int Status { get; set; }
    public string? ManufacturerDecision { get; set; }
    public bool IsRecall { get; set; }
    public decimal TotalPartsCost { get; set; }
    public decimal TotalLaborCost { get; set; }
    public List<WarrantyClaimPartResponse> Parts { get; set; } = new();
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public class WarrantyClaimDetailResponse
{
    public int Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string? VehicleInfo { get; set; }
    public string? VehiclePlate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? VehicleVin { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public List<string> MediaUrls { get; set; } = new();
    public string? ServiceCenterName { get; set; }
    public string? ManufacturerClaimNumber { get; set; }
    public string? ManufacturerDecision { get; set; }
    public bool IsRecall { get; set; }
    public decimal TotalPartsCost { get; set; }
    public decimal TotalLaborCost { get; set; }
    public List<WarrantyClaimPartResponse> Parts { get; set; } = new();
    public string? VehicleColor { get; set; }
    public string? VehicleYear { get; set; }
    public string? WarrantyRemaining { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
