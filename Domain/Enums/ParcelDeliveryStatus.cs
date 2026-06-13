using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum ParcelDeliveryStatus
    {
        Pending = 0,      // Chờ soạn
        Packing = 1,      // Đang gói
        Shipping = 2,     // Đang đi đường
        Completed = 3,    // Giao thành công
        Returned = 4      // Khách bom hàng/Chuyển hoàn
    }
}
