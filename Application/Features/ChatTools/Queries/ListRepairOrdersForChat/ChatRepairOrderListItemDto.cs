namespace Application.Features.ChatTools.Queries.ListRepairOrdersForChat;

public record ChatRepairOrderListItemDto
{
    public int RepairOrderId { get; init; }

    public string? VehicleInfo { get; init; }

    public string? CustomerName { get; init; }

    public DateTimeOffset MaintenanceDate { get; init; }
}
