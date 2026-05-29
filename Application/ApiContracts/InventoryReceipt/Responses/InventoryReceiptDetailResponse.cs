
namespace Application.ApiContracts.InventoryReceipt.Responses;

public class InventoryReceiptDetailResponse
{
    public int? Id { get; set; }

    public int? PurchaseRequestId { get; set; }

    public string? Notes { get; set; }

    public string? StatusId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public long? TotalPayable { get; set; }

    public List<InventoryReceiptInfoResponse> Products { get; set; } = [];
}
