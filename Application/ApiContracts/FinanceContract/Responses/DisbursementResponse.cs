using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.FinanceContract.Responses
{
    public class DisbursementResponse
    {
        public DateTime? ExpectedDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public decimal? ExpectedAmount { get; set; }
        public decimal? ActualAmount { get; set; }
    }

}
