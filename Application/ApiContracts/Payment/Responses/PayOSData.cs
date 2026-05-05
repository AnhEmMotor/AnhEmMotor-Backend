
namespace Application.ApiContracts.Payment.Responses;

public class PayOSData
{
    public long OrderCode { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CheckoutUrl { get; set; } = string.Empty;

    public string PaymentLinkId { get; set; } = string.Empty;
}
