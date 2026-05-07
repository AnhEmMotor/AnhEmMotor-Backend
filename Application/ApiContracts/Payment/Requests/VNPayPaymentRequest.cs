using System;

namespace Application.ApiContracts.Payment.Requests
{
    public class VNPayPaymentRequest
    {
        public int OrderId { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
