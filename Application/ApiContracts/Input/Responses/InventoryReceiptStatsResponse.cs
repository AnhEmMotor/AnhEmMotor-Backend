namespace Application.ApiContracts.Input.Responses;

public class InventoryReceiptStatsResponse
{
    public int TotalVehicles { get; set; }
    public int ProcessingReceipts { get; set; }
    public decimal TotalValue { get; set; }
}
