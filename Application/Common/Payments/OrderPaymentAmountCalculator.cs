using Domain.Constants.Order;
using Domain.Entities;

namespace Application.Common.Payments;

public static class OrderPaymentAmountCalculator
{
    public static decimal GetTotal(Output order)
    {
        var subtotal = order.OutputInfos.Sum(info => (info.Price ?? 0) * (info.Count ?? 0));
        var shippingFee = subtotal > 0 && subtotal <= 10000000 ? 200000 : 0;
        return subtotal + shippingFee;
    }

    public static decimal GetDepositAmount(Output order) => GetTotal(order) * (order.DepositRatio ?? 0) / 100m;

    public static decimal GetAmountToPay(Output order) => string.Equals(
            order.StatusId,
            OrderStatus.WaitingDeposit,
            StringComparison.OrdinalIgnoreCase)
        ? GetDepositAmount(order)
        : GetTotal(order);
}
