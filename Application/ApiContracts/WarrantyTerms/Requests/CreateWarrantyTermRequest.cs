namespace Application.ApiContracts.WarrantyTerms.Requests;

public class CreateWarrantyTermRequest
{
    public int BrandId { get; set; }
    public string TermName { get; set; } = null!;
    public string? TermNameJson { get; set; }
    public string? VehicleType { get; set; }
    public string? ErrorCategory { get; set; }
    public string? Description { get; set; }
    public string? DescriptionJson { get; set; }
    public int? DurationMonths { get; set; }
    public int? DurationKm { get; set; }
    public string? Coverage { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? MediaUrl { get; set; }
}

public class UpdateWarrantyTermRequest : CreateWarrantyTermRequest
{
}
