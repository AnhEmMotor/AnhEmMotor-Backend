using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.HR;

[Table("CommissionRecord")]
public class CommissionRecord : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmployeeProfileId { get; set; }

    [ForeignKey("EmployeeProfileId")]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Required]
    public int OutputId { get; set; }

    [ForeignKey("OutputId")]
    public Output Output { get; set; } = null!;

    /// <summary>
    /// Tổng tiền hoa hồng của đơn hàng này
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Trạng thái: Pending → Confirmed → Paid
    /// </summary>
    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;

    public DateTime DateEarned { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời điểm Admin duyệt chi trả
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Ảnh chụp (Snapshot) chi tiết tính toán tại thời điểm ghi nhận. Bất biến — không thay đổi dù Admin cập nhật chính
    /// sách về sau.
    /// </summary>
    [Column(TypeName = "nvarchar(MAX)")]
    public string? PolicySnapshot { get; set; }

    [Column(TypeName = "nvarchar(255)")]
    public string? Note { get; set; }
}

