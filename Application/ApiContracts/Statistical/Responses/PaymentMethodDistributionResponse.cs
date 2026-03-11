using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class PaymentMethodDistributionResponse
    {
        public string? MethodName { get; set; }

        public decimal Value { get; set; }
    }
}
