namespace Application.ApiContracts.InventoryReceipt.Responses;

public class InventoryReceiptInfoAuditLogResponse
{
    public string Action { get; set; } = string.Empty;
    public int? OldQuantity { get; set; }
    public int? NewQuantity { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal? NewPrice { get; set; }
}
