using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Logistics;

public class CarrierPartner
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string CarrierCode { get; set; } = string.Empty; // internal stable code (ghtk, ghn, viettel-post...)

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    // "sandbox" | "production"
    [Required]
    public string Environment { get; set; } = "sandbox";

    public string ApiBaseUrl { get; set; } = string.Empty;

    // Stored secrets (should be encrypted in production; for now keep plain in DB as placeholder)
    public string ApiToken { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    public string WebhookEndpointUrl { get; set; } = string.Empty;

    public bool AutoSyncPricing { get; set; }
    public decimal MaxParcelWeightKg { get; set; }

    public bool AllowLiquidCargo { get; set; }
    public bool AllowOversizeCargo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

