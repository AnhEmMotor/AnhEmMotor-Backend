using System;

namespace Application.ApiContracts.FinanceContract.Responses
{
    public class PartnerResponse
    {
        public string Name { get; set; } = string.Empty;

        public string? ContactPhone { get; set; }

        public string? BankStaffName { get; set; }
    }
}
