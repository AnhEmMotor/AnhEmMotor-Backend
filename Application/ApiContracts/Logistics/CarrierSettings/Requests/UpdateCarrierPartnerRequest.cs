namespace Application.ApiContracts.Logistics.CarrierSettings.Requests;

public class UpdateCarrierPartnerRequest
{
    public bool IsActive { get; set; }
    public string Environment { get; set; } = string.Empty; // "sandbox" | "production"

    public string ApiBaseUrl { get; set; } = string.Empty;

    // For security UI sends masked tokens; backend expects optional new secrets.
    public string? ApiTokenPlain { get; set; }
    public string? WebhookSecretPlain { get; set; }

    public string WebhookEndpointUrl { get; set; } = string.Empty;

    public bool AutoSyncPricing { get; set; }
    public decimal MaxParcelWeightKg { get; set; }

    public bool AllowLiquidCargo { get; set; }
    public bool AllowOversizeCargo { get; set; }
}

