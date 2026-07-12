namespace Application.ApiContracts.Vehicle.Responses;

public class VehiclePortfolioResponse
{
    public VehicleResponse Vehicle { get; set; } = default!;

    public List<VehiclePortfolioHistoryItem> History { get; set; } = new();

    public int TotalHistoryCount { get; set; }
}

public class VehiclePortfolioHistoryItem
{
    public int Id { get; set; }

    public string? MaintenanceNumber { get; set; }

    public int VehicleId { get; set; }

    public string? VehicleInfo { get; set; }

    public DateTimeOffset MaintenanceDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public int Mileage { get; set; }

    public string? TechnicianName { get; set; }

    public decimal PartsCost { get; set; }

    public decimal LaborCost { get; set; }

    public decimal TotalCost { get; set; }

    public DateTimeOffset? NextMaintenanceDate { get; set; }

    public int? NextMaintenanceOdo { get; set; }

    public string Status { get; set; } = "Completed";

    public string? PartsJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public List<PortfolioPartItem> Details { get; set; } = new();
}

public class PortfolioPartItem
{
    public string Type { get; set; } = "Part";

    public string? VariantName { get; set; }

    public string? ProductCode { get; set; }

    public int Count { get; set; }
}
