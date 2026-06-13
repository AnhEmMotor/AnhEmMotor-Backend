using System.ComponentModel;

namespace Domain.Constants.Workshop;

public enum WorkshopTicketStatus
{
    [Description("Chờ tiếp nhận")]
    Pending = 0,
    [Description("Đang sửa chữa")]
    InProgress = 1,
    [Description("Đang kiểm tra")]
    Checking = 2,
    [Description("Đã hoàn tất")]
    Completed = 3,
    [Description("Đã hủy")]
    Cancelled = 4
}
