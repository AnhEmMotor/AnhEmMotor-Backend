namespace Domain.Constants.Order;

public static class PaymentMethod
{
    public const string COD = "COD";
    public const string VNPay = "VNPay";
    public const string PayOS = "PayOS";

    public static readonly HashSet<string> All = [COD, VNPay, PayOS];

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        return All.Contains(value);
    }
}
