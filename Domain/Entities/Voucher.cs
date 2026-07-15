using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Voucher : BaseEntity
{
	[Key]
	[Column("Id")]
	public int Id { get; set; }

	public string Code { get; set; } = null!;

	public string Name { get; set; } = null!;

	public VoucherApplyFor ApplyFor { get; set; }

	public VoucherChannel Channel { get; set; }

	public VoucherType Type { get; set; }

	public DiscountType DiscountType { get; set; }

	public decimal DiscountValue { get; set; }

	public decimal? MaxDiscountAmount { get; set; }

	/// <summary>
	/// Giá trị đơn hàng tối thiểu để áp dụng voucher.
	/// </summary>
	[Column(TypeName = "decimal(18,2)")]
	public decimal MinOrderValue { get; set; } = 0;

	/// <summary>
	/// Giới hạn mỗi khách hàng dùng bao nhiêu lần (0 = không giới hạn).
	/// </summary>
	public int UsageLimitPerUser { get; set; } = 1;

	/// <summary>
	/// Tổng số lần voucher có thể dùng (0 = không giới hạn).
	/// </summary>
	public int TotalUsageLimit { get; set; } = 0;

	/// <summary>
	/// Số lần đã dùng rồi (tự tăng khi áp dụng).
	/// </summary>
	public int UsedCount { get; set; } = 0;

	public DateTime ValidFrom { get; set; }

	public DateTime ValidTo { get; set; }

	public virtual ICollection<VoucherLead> VoucherLeads { get; set; } = new List<VoucherLead>();

	public virtual ICollection<OrderVoucher> OrderVouchers { get; set; } = new List<OrderVoucher>();
}
