using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.FinanceContract.Responses
{
    public class CavetResponse
    {
        public string State { get; set; } = string.Empty;
        public DateTime? ReceivedDate { get; set; }
        public string? ReceiverName { get; set; }
        public string? StorageLocation { get; set; }
    }
}
