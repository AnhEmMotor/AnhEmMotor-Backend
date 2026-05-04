
namespace Application.ApiContracts.Payment.Requests;

public class PayOSPaymentRequest
{
    public int OrderId { get; set; }

    public long OrderCode { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;
}
