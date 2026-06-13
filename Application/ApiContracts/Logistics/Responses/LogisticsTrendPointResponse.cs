using System;

namespace Application.ApiContracts.Logistics.Responses
{
    public class LogisticsTrendPointResponse
    {
        public string DayLabel { get; set; } = string.Empty;

        public int DeliveredCount { get; set; }

        public decimal ShippingCost { get; set; }
    }
}
