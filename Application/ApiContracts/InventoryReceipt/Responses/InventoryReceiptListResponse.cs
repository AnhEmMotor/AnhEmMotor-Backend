using System.Text.Json.Serialization;

namespace Application.ApiContracts.InventoryReceipt.Responses;

public class InventoryReceiptListResponse
{
    public int? Id { get; set; }

    public string? Notes { get; set; }

    public string? StatusId { get; set; }

    public string? CreatedByName { get; set; }

    public string? SentByName { get; set; }

    public string? ApprovedByName { get; set; }

    public string? RejectedByName { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public List<InventoryReceiptInfoResponse> Products { get; set; } = [];

    [JsonPropertyName("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }
}
