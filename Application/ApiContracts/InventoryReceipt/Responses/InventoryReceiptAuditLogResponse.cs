using System;
using System.Collections.Generic;

namespace Application.ApiContracts.InventoryReceipt.Responses;

public class InventoryReceiptAuditLogResponse
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ChangedByFullName { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? OldStatusId { get; set; }
    public string? NewStatusId { get; set; }
    public string? OldNotes { get; set; }
    public string? NewNotes { get; set; }
    
    public List<InventoryReceiptInfoAuditLogResponse> InfoLogs { get; set; } = new();
    public List<VehicleAuditLogResponse> VehicleLogs { get; set; } = new();
}
