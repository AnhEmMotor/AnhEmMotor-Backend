using System;

namespace Application.ApiContracts.Payment.Responses
{
    public class VNPayPaymentResponse
    {
        public bool Success { get; set; }

        public string PaymentMethod { get; set; } = "VNPay";

        public string OrderDescription { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty;

        public string PaymentId { get; set; } = string.Empty;

        public string TransactionId { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string VnPayResponseCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }
}
