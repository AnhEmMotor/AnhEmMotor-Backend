using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Logistics.Responses
{
    public class LogisticsDashboardSummaryResponse
    {
        public int FulfillmentWorkload { get; set; }
        public decimal PendingUnreconciledCod { get; set; }
        public double OtifRate { get; set; }
        public double ReturnsClaimsRate { get; set; }
        public bool FulfillmentWorkloadIsOverload { get; set; }
    }
}
