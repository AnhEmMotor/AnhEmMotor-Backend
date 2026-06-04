
namespace Application.ApiContracts.InventoryReceipt.Responses;

public class InventoryReceiptInfoResponse
{
    public int? Id { get; set; }

    public int? ProductVariantId { get; set; }

    public int? ProductVariantColorId { get; set; }

    public string? ProductVariantColorName { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public string? Name { get; set; }

    public int? Quantity { get; set; }

    public int? RemainingCount { get; set; }

    public int? OrderedQuantity { get; set; }

    public int? MaxAllowedQuantity { get; set; }

    public int? PurchaseOrderItemId { get; set; }

    public List<InventoryReceiptVehicleResponse> Vehicles { get; set; } = [];
}
