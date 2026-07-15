using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

/// <summary>
/// Bang trung gian lien ket Voucher voi Output (don hang).
/// Dung de tracking viec ap dung voucher cho tung don hang cu the.
/// </summary>
public class OrderVoucher
{
    public int Id { get; set; }

    public int VoucherId { get; set; }
    public virtual Voucher Voucher { get; set; } = null!;

    /// <summary>
    /// OutputId dai dien cho don hang (SalesContract -> Output).
    /// </summary>
    public int OutputId { get; set; }
    public virtual Output? Output { get; set; }

    /// <summary>
    /// So tien giam gia thuc te khi ap dung voucher cho don nay.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountApplied { get; set; }

    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;

    public string AppliedBy { get; set; } = string.Empty;
}
