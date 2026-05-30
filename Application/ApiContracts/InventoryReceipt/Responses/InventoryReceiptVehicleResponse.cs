
namespace Application.ApiContracts.InventoryReceipt.Responses;

public class InventoryReceiptVehicleResponse
{
    public int? Id { get; set; }

    public string? VinNumber { get; set; }

    public string? EngineNumber { get; set; }

    public int? ProductVariantId { get; set; }

    public int? ProductVariantColorId { get; set; }
}
