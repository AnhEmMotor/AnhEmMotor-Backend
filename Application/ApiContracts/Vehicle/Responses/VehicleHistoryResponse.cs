namespace Application.ApiContracts.Vehicle.Responses;

public class VehicleHistoryResponse
{
    public List<VehiclePurchaseHistoryItem> PurchaseHistory { get; set; } = [];
    public List<VehicleWarrantyHistoryItem> WarrantyHistory { get; set; } = [];
}

public class VehiclePurchaseHistoryItem
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Guid? UserId { get; set; }
    public DateTimeOffset PurchaseDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class VehicleWarrantyHistoryItem
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Guid? UserId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }
}
