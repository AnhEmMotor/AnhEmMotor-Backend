namespace Application.ApiContracts.Admin.Workshop.Responses;

public class RepairOrderResponse
{
    public int Id { get; set; }

    public string MaintenanceNumber { get; set; } = string.Empty;

    public int VehicleId { get; set; }

    public string? VehicleInfo { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerPhone { get; set; }

    public DateTimeOffset MaintenanceDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public int Mileage { get; set; }

    public int? TechnicianId { get; set; }

    public string? TechnicianName { get; set; }

    public decimal PartsCost { get; set; }

    public decimal LaborCost { get; set; }

    public decimal TotalCost { get; set; }

    public string? PartsJson { get; set; }

    public DateTimeOffset? NextMaintenanceDate { get; set; }

    public int? NextMaintenanceOdo { get; set; }

    public string? ServiceType { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string? VoucherCode { get; set; }

    public decimal? VoucherDiscount { get; set; }

    public decimal? VoucherFinalTotal { get; set; }

    public string? ProductImage { get; set; }

    public string? VehicleName { get; set; }
    
    public string? CategoryName { get; set; }
    
    public string? VariantName { get; set; }
    
    public string? ColorName { get; set; }
    
    public string? VinNumber { get; set; }

    public DateTimeOffset? ExpectedCompletionDate { get; set; }
}
