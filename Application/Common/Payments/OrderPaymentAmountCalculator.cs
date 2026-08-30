using Domain.Constants.Order;
using Domain.Entities;

namespace Application.Common.Payments;

public static class OrderPaymentAmountCalculator
{
    public static decimal GetSubtotal(Output order)
    {
        return order.OutputInfos.Sum(info => (info.Price ?? 0) * (info.Count ?? 0));
    }

    public static decimal GetTotal(Output order)
    {
        var subtotal = GetSubtotal(order);
        var shippingFee = order.ShippingFee ?? 0;
        var discount = order.OrderVouchers?.Sum(v => v.DiscountApplied) ?? 0m;
        return Math.Max(0, subtotal + shippingFee - discount);
    }

    public static decimal GetDepositAmount(Output order) => GetTotal(order) * (order.DepositRatio ?? 0) / 100m;

    public static decimal GetAmountToPay(Output order) => string.Equals(
            order.StatusId,
            OrderStatus.WaitingDeposit,
            StringComparison.OrdinalIgnoreCase)
        ? GetDepositAmount(order)
        : GetTotal(order);
}
