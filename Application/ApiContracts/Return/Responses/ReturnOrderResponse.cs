using Application.Features.Logistics.Returns;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Return.Responses
{
    public class ReturnOrderResponse
    {
            public int Id { get; set; }
            public string OriginalTrackingNumber { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public string Carrier { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public ReturnOrderStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
    }
}
