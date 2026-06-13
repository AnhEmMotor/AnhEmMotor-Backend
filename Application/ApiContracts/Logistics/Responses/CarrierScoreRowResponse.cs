using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Logistics.Responses
{
    public class CarrierScoreRowResponse
    {
        public string Carrier { get; set; } = string.Empty;
        public int DeliveredCount { get; set; }
        public double AvgDeliveryDays { get; set; }
        public decimal AvgShippingCostPerOrder { get; set; }
        public double ReturnsRatio { get; set; }
    }
}
