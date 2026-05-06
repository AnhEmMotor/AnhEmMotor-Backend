namespace Domain.Entities.HR;

/// <summary>
/// Trạng thái hoa hồng theo luồng nghiệp vụ
/// </summary>
public enum CommissionStatus
{
    /// <summary>Tạm tính - Khi đơn hàng đang Processing/Confirmed</summary>
    Pending = 0,
    /// <summary>Ghi nhận - Khi đơn hàng chuyển sang Completed</summary>
    Confirmed = 1,
    /// <summary>Đã chi trả - Sau khi Admin duyệt chi trả cuối tháng</summary>
    Paid = 2
}
