namespace Application.ApiContracts.Shipping.Requests;

public class GhnWebhookRequest
{
    public int CODAmount { get; set; }
    public string ClientOrderCode { get; set; } = string.Empty;
    public int ConvertedWeight { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Height { get; set; }
    public bool IsPartialReturn { get; set; }
    public int Length { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string PartialReturnCode { get; set; } = string.Empty;
    public int PaymentType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ReasonCode { get; set; } = string.Empty;
    public int ShopID { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public int TotalFee { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public int Weight { get; set; }
    public int Width { get; set; }
}
