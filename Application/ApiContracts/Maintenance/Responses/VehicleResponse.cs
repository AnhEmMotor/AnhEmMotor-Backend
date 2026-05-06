namespace Application.ApiContracts.Maintenance.Responses;

public class VehicleResponse
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public string VinNumber { get; set; } = string.Empty;
    public string EngineNumber { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DateTimeOffset PurchaseDate { get; set; }
    public List<VehicleDocumentResponse> Documents { get; set; } = new();
}
