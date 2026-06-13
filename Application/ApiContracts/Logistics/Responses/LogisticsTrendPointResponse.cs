using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Logistics.Responses
{
    public class LogisticsTrendPointResponse
    {
        public string DayLabel { get; set; } = string.Empty;
        public int DeliveredCount { get; set; }
        public decimal ShippingCost { get; set; }
    }
}
