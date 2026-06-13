using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Logistics.Responses
{
    public class LogisticsExceptionRowResponse
    {
        public string Type { get; set; } = string.Empty; // ngâm kho / giao chậm / hoàn chờ kiểm tra
        public string TrackingNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
