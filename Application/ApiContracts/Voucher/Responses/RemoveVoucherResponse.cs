namespace Application.ApiContracts.Voucher.Responses;

/// <summary>
/// Response returned after removing (un-applying) a voucher from an order.
/// </summary>
public class RemoveVoucherResponse
{
    public int OrderVoucherId { get; set; }
    public decimal RefundedAmount { get; set; }
}
