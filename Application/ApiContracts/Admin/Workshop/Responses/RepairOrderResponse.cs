namespace Application.ApiContracts.Admin.Workshop.Responses;

public class RepairOrderResponse
{
    public int Id { get; set; }
    public string MaintenanceNumber { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string? VehicleInfo { get; set; }
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
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
