
namespace Application.ApiContracts.InventoryReceipt.Responses;

public class InventoryReceiptDetailResponse
{
    public int? Id { get; set; }

    public string? Notes { get; set; }

    public string? StatusId { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public string? SupplierPhone { get; set; }

    public string? SupplierEmail { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public long? TotalPayable { get; set; }

    public List<InventoryReceiptInfoResponse> Products { get; set; } = [];
}
