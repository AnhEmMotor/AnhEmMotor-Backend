namespace Domain.Constants
{
    public enum BookingStatus
    {
        PendingAllocation = 0, // Chờ phân bổ
        Confirmed = 1,         // Đã xác nhận
        InProgress = 2,        // Đang thực hiện
        Completed = 3,         // Đã hoàn thành
        Cancelled = 4,         // Đã hủy
        NoShow = 5             // Khách không đến
    }
}
