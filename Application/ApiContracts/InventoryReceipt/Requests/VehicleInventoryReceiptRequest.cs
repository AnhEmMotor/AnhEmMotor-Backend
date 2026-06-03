namespace Application.ApiContracts.InventoryReceipt.Requests
{
    public class VehicleInventoryReceiptRequest
    {
        public int? Id { get; set; }

        public string VinNumber { get; set; } = string.Empty;

        public string EngineNumber { get; set; } = string.Empty;

        public decimal ImportPrice { get; set; } = 0;
    }
}
