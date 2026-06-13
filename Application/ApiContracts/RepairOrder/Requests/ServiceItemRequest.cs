using System;

namespace Application.ApiContracts.RepairOrder.Requests
{
    public class ServiceItemRequest
    {
        public int ServiceId { get; set; }

        public decimal LaborCost { get; set; }

        public string? Notes { get; set; }
    }
}
