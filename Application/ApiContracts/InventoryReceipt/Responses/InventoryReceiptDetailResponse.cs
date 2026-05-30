
namespace Application.ApiContracts.InventoryReceipt.Responses;

public class InventoryReceiptDetailResponse
{
    public int? Id { get; set; }

    public int? PurchaseRequestId { get; set; }

    public string? Notes { get; set; }

    public string? StatusId { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public decimal? PaidAmount { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public long? TotalPayable { get; set; }

    public List<InventoryReceiptInfoResponse> Products { get; set; } = [];
}
