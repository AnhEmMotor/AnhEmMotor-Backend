using Domain.Entities.Logistics;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class CarrierPartnerSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        if (await context.CarrierPartners.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;
        var carriers = new List<CarrierPartner>
        {
            new()
            {
                CarrierCode = "ghtk",
                Name = "Giao Hàng Tiết Kiệm",
                IsActive = true,
                Environment = "sandbox",
                ApiBaseUrl = "https://services.giaohangtietkiem.vn/api/v1",
                ApiToken = "demo-token-ghtk",
                WebhookSecret = "demo-secret-ghtk",
                WebhookEndpointUrl = "https://api.anhemmotor.com/v1/webhooks/ghtk",
                AutoSyncPricing = true,
                MaxParcelWeightKg = 25,
                AllowLiquidCargo = true,
                AllowOversizeCargo = false,
                PricingRulesJson = @"[
  {""routeType"": ""IntraProvince"", ""weightTier"": ""0-2kg"", ""price"": 22000},
  {""routeType"": ""IntraProvince"", ""weightTier"": ""2-5kg"", ""price"": 35000},
  {""routeType"": ""IntraProvince"", ""weightTier"": "">5kg"", ""price"": 50000},
  {""routeType"": ""IntraRegion"", ""weightTier"": ""0-2kg"", ""price"": 30000},
  {""routeType"": ""IntraRegion"", ""weightTier"": ""2-5kg"", ""price"": 45000},
  {""routeType"": ""IntraRegion"", ""weightTier"": "">5kg"", ""price"": 70000},
  {""routeType"": ""InterRegion"", ""weightTier"": ""0-2kg"", ""price"": 40000},
  {""routeType"": ""InterRegion"", ""weightTier"": ""2-5kg"", ""price"": 65000},
  {""routeType"": ""InterRegion"", ""weightTier"": "">5kg"", ""price"": 95000}
]",
                SlaJson = @"[
  {""routeType"": ""IntraProvince"", ""expectedDays"": ""1-2 ngày""},
  {""routeType"": ""IntraRegion"", ""expectedDays"": ""2-3 ngày""},
  {""routeType"": ""InterRegion"", ""expectedDays"": ""3-5 ngày""}
]"
            },
            new()
            {
                CarrierCode = "ghn",
                Name = "Giao Hàng Nhanh",
                IsActive = true,
                Environment = "sandbox",
                ApiBaseUrl = "https://online-gateway.ghn.vn/shiip/public-api/v1",
                ApiToken = "demo-token-ghn",
                WebhookSecret = "demo-secret-ghn",
                WebhookEndpointUrl = "https://api.anhemmotor.com/v1/webhooks/ghn",
                AutoSyncPricing = true,
                MaxParcelWeightKg = 30,
                AllowLiquidCargo = false,
                AllowOversizeCargo = true,
                PricingRulesJson = @"[
  {""routeType"": ""IntraProvince"", ""weightTier"": ""0-2kg"", ""price"": 24000},
  {""routeType"": ""IntraProvince"", ""weightTier"": ""2-5kg"", ""price"": 38000},
  {""routeType"": ""IntraProvince"", ""weightTier"": "">5kg"", ""price"": 55000},
  {""routeType"": ""IntraRegion"", ""weightTier"": ""0-2kg"", ""price"": 32000},
  {""routeType"": ""IntraRegion"", ""weightTier"": ""2-5kg"", ""price"": 48000},
  {""routeType"": ""IntraRegion"", ""weightTier"": "">5kg"", ""price"": 75000},
  {""routeType"": ""InterRegion"", ""weightTier"": ""0-2kg"", ""price"": 42000},
  {""routeType"": ""InterRegion"", ""weightTier"": ""2-5kg"", ""price"": 68000},
  {""routeType"": ""InterRegion"", ""weightTier"": "">5kg"", ""price"": 99000}
]",
                SlaJson = @"[
  {""routeType"": ""IntraProvince"", ""expectedDays"": ""1-2 ngày""},
  {""routeType"": ""IntraRegion"", ""expectedDays"": ""2-3 ngày""},
  {""routeType"": ""InterRegion"", ""expectedDays"": ""3-4 ngày""}
]"
            },
            new()
            {
                CarrierCode = "viettel-post",
                Name = "Viettel Post",
                IsActive = false,
                Environment = "sandbox",
                ApiBaseUrl = "https://api.viettelpost.vn/api",
                ApiToken = "demo-token-viettel",
                WebhookSecret = "demo-secret-viettel",
                WebhookEndpointUrl = "https://api.anhemmotor.com/v1/webhooks/viettel-post",
                AutoSyncPricing = true,
                MaxParcelWeightKg = 50,
                AllowLiquidCargo = false,
                AllowOversizeCargo = true,
                PricingRulesJson = @"[
  {""routeType"": ""IntraProvince"", ""weightTier"": ""0-2kg"", ""price"": 20000},
  {""routeType"": ""IntraProvince"", ""weightTier"": ""2-5kg"", ""price"": 32000},
  {""routeType"": ""IntraProvince"", ""weightTier"": "">5kg"", ""price"": 45000},
  {""routeType"": ""IntraRegion"", ""weightTier"": ""0-2kg"", ""price"": 28000},
  {""routeType"": ""IntraRegion"", ""weightTier"": ""2-5kg"", ""price"": 42000},
  {""routeType"": ""IntraRegion"", ""weightTier"": "">5kg"", ""price"": 65000},
  {""routeType"": ""InterRegion"", ""weightTier"": ""0-2kg"", ""price"": 38000},
  {""routeType"": ""InterRegion"", ""weightTier"": ""2-5kg"", ""price"": 60000},
  {""routeType"": ""InterRegion"", ""weightTier"": "">5kg"", ""price"": 90000}
]",
                SlaJson = @"[
  {""routeType"": ""IntraProvince"", ""expectedDays"": ""1-3 ngày""},
  {""routeType"": ""IntraRegion"", ""expectedDays"": ""2-4 ngày""},
  {""routeType"": ""InterRegion"", ""expectedDays"": ""3-6 ngày""}
]"
            },
            new()
            {
                CarrierCode = "shipper-nha",
                Name = "Shipper Nhà",
                IsActive = false,
                Environment = "sandbox",
                ApiBaseUrl = string.Empty,
                ApiToken = string.Empty,
                WebhookSecret = string.Empty,
                WebhookEndpointUrl = string.Empty,
                AutoSyncPricing = false,
                MaxParcelWeightKg = 20,
                AllowLiquidCargo = true,
                AllowOversizeCargo = false,
                PricingRulesJson = @"[
  {""routeType"": ""IntraProvince"", ""weightTier"": ""0-2kg"", ""price"": 15000},
  {""routeType"": ""IntraProvince"", ""weightTier"": ""2-5kg"", ""price"": 25000},
  {""routeType"": ""IntraProvince"", ""weightTier"": "">5kg"", ""price"": 35000},
  {""routeType"": ""IntraRegion"", ""weightTier"": ""0-2kg"", ""price"": 25000},
  {""routeType"": ""IntraRegion"", ""weightTier"": ""2-5kg"", ""price"": 35000},
  {""routeType"": ""IntraRegion"", ""weightTier"": "">5kg"", ""price"": 50000},
  {""routeType"": ""InterRegion"", ""weightTier"": ""0-2kg"", ""price"": 50000},
  {""routeType"": ""InterRegion"", ""weightTier"": ""2-5kg"", ""price"": 70000},
  {""routeType"": ""InterRegion"", ""weightTier"": "">5kg"", ""price"": 100000}
]",
                SlaJson = @"[
  {""routeType"": ""IntraProvince"", ""expectedDays"": ""Trong ngày""},
  {""routeType"": ""IntraRegion"", ""expectedDays"": ""1 ngày""},
  {""routeType"": ""InterRegion"", ""expectedDays"": ""2 ngày""}
]"
            },
        };
        foreach (var carrier in carriers)
        {
            var existing = await context.CarrierPartners
                .FirstOrDefaultAsync(c => string.Compare(c.CarrierCode, carrier.CarrierCode) == 0, cancellationToken)
                .ConfigureAwait(false);
            if (existing == null)
            {
                await context.CarrierPartners.AddAsync(carrier, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (string.IsNullOrEmpty(existing.PricingRulesJson))
                    existing.PricingRulesJson = carrier.PricingRulesJson;
                if (string.IsNullOrEmpty(existing.SlaJson))
                    existing.SlaJson = carrier.SlaJson;
                context.CarrierPartners.Update(existing);
            }
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
