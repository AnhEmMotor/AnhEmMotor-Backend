using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.RepairOrder.Requests
{
    public class ServiceItemRequest
    {
            public int ServiceId { get; set; }
            public decimal LaborCost { get; set; }
            public string? Notes { get; set; }
    }
}
