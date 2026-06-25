using System;

namespace Application.ApiContracts.Logistics.CarrierSettings.Responses
{
    public class CarrierPartnerItemResponse
    {
        public int Id { get; set; }

        public string CarrierCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string Environment { get; set; } = string.Empty;

        public string ApiBaseUrl { get; set; } = string.Empty;

        public string ApiTokenMasked { get; set; } = string.Empty;

        public string ApiTokenPlain { get; set; } = string.Empty;

        public string WebhookSecretMasked { get; set; } = string.Empty;

        public string WebhookSecretPlain { get; set; } = string.Empty;

        public string WebhookEndpointUrl { get; set; } = string.Empty;

        public bool AutoSyncPricing { get; set; }

        public decimal MaxParcelWeightKg { get; set; }

        public bool AllowLiquidCargo { get; set; }

        public bool AllowOversizeCargo { get; set; }
    }
}
