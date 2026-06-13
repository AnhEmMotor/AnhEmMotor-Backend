using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.FinanceContract.Responses
{
    public class Customer360Response
    {
        public string FullName { get; set; } = string.Empty;
        public string Cccd { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
