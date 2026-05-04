using System;

namespace Infrastructure.Integrations.Payment.PayOS
{
    public class PayOSApiResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Desc { get; set; } = string.Empty;

        public PayOSInternalData Data { get; set; } = new();
    }
}
