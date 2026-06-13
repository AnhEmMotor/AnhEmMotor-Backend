using System;

namespace Application.ApiContracts.Logistics.Responses
{
    public class LogisticsExceptionRowResponse
    {
        public string Type { get; set; } = string.Empty;

        public string TrackingNumber { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
