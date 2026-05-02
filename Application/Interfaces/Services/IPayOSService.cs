namespace Application.Interfaces.Services;

public interface IPayOSService
{
    Task<PayOSPaymentResponse> CreatePaymentAsync(PayOSPaymentRequest request);
    bool VerifyWebhook(PayOSWebhookData data);
    Task<PayOSData?> GetPaymentDetailsAsync(long orderCode);
}

public class PayOSData
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string PaymentLinkId { get; set; } = string.Empty;
}

public class PayOSPaymentRequest
{
    public int OrderId { get; set; }
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class PayOSPaymentResponse
{
    public int ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string PaymentLinkId { get; set; } = string.Empty;
}

public class PayOSWebhookData
{
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
