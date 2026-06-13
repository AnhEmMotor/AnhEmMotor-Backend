using System;

namespace Application.ApiContracts.FinanceContract.Responses
{
    public class CreditPackageResponse
    {
        public string? ContractNo { get; set; }

        public decimal? PrincipalAmount { get; set; }

        public int? TermMonths { get; set; }

        public string? InterestRateRange { get; set; }

        public decimal? MonthlyPaymentAmount { get; set; }
    }
}
