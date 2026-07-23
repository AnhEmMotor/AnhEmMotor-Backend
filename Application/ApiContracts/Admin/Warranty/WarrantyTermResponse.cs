namespace Application.ApiContracts.Admin.Warranty;

public class WarrantyTermResponse
{
    public int Id { get; set; }

    public int BrandId { get; set; }

    public string BrandName { get; set; } = string.Empty;

    public string TermName { get; set; } = string.Empty;

    public string TermNameJson { get; set; } = string.Empty;

    public string VehicleType { get; set; } = string.Empty;

    public string ErrorCategory { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? DescriptionJson { get; set; }

    public int? DurationMonths { get; set; }

    public int? DurationKm { get; set; }

    public string? Coverage { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? EffectiveDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public string? MediaUrl { get; set; }
}
