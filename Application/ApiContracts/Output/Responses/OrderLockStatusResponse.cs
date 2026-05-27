
namespace Application.ApiContracts.Output.Responses;

public class OrderLockStatusResponse
{
    public HashSet<string> BuyerAndProducts { get; set; } = [];

    public HashSet<string> DeliveryInfo { get; set; } = [];

    public HashSet<string> Notes { get; set; } = [];

    public HashSet<string> DepositRatio { get; set; } = [];

    public HashSet<string> PaymentLink { get; set; } = [];
}
