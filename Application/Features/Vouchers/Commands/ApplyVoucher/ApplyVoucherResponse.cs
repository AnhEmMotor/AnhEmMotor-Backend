namespace Application.Features.Vouchers.Commands.ApplyVoucher;

/// <summary>
/// Response trả về sau khi áp dụng voucher thành công.
/// </summary>
public class ApplyVoucherResponse
{
	public int OrderVoucherId { get; set; }
	public string VoucherCode { get; set; } = string.Empty;
	public string VoucherName { get; set; } = string.Empty;
	public decimal DiscountAmount { get; set; }
	public DateTimeOffset AppliedAt { get; set; }
}
