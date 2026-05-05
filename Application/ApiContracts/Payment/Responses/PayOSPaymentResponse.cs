
namespace Application.ApiContracts.Payment.Responses;

public class PayOSPaymentResponse
{
    public int ErrorCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public string CheckoutUrl { get; set; } = string.Empty;

    public string PaymentLinkId { get; set; } = string.Empty;
}
