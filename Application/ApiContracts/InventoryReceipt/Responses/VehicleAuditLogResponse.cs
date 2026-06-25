using System;

namespace Application.ApiContracts.InventoryReceipt.Responses;

public class VehicleAuditLogResponse
{
    public string Action { get; set; } = string.Empty;

    public string? ChangedByFullName { get; set; }

    public DateTimeOffset ChangedAt { get; set; }

    public string? OldVinNumber { get; set; }

    public string? NewVinNumber { get; set; }

    public string? OldEngineNumber { get; set; }

    public string? NewEngineNumber { get; set; }

    public string? ProductVariantName { get; set; }
}
