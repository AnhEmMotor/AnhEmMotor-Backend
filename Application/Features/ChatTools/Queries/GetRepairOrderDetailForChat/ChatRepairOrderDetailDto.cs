namespace Application.Features.ChatTools.Queries.GetRepairOrderDetailForChat;

public record ChatRepairOrderDetailDto
{
    public int RepairOrderId { get; init; }

    public string? MaintenanceNumber { get; init; }

    public string? VehicleInfo { get; init; }

    public string? CustomerName { get; init; }

    public string? TechnicianName { get; init; }

    public string? Description { get; init; }

    public string? PartsJson { get; init; }

    public decimal PartsCost { get; init; }

    public decimal LaborCost { get; init; }

    public decimal TotalCost { get; init; }

    public string Currency { get; init; } = "VND";

    public DateTimeOffset MaintenanceDate { get; init; }
}
