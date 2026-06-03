namespace Application.ApiContracts.PurchaseOrder.Responses
{
    public class PurchaseOrderVehicleInputResponse
    {
        public int Id { get; set; }
        public string VinNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public int? PurchaseInvoiceItemId { get; set; }
    }
}
