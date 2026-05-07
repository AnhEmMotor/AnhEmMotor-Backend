
namespace Application.ApiContracts.Payment.Requests;

public class PayOSWebhookData
{
    public string OrderCode { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public string TransactionId { get; set; } = string.Empty;

    public string Signature { get; set; } = string.Empty;
}
