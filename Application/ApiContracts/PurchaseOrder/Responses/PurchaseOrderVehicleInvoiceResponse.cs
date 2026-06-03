namespace Application.ApiContracts.PurchaseOrder.Responses
{
    public class PurchaseOrderVehicleInvoiceResponse
    {
        public int Id { get; set; }
        public string VinNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public decimal ImportPrice { get; set; }
        public int? InventoryReceiptInfoId { get; set; }
        public bool IsLocked => InventoryReceiptInfoId.HasValue;
    }
}
