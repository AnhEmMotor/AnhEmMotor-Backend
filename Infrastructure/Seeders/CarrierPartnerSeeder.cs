using Domain.Entities.Logistics;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class CarrierPartnerSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        if (await context.CarrierPartners.AnyAsync(cancellationToken))
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
            },
            new()
            {
                CarrierCode = "shipper-nha",
                Name = "Shipper Nhà",
                IsActive = false,
                Environment = "sandbox",
                ApiBaseUrl = "",
                ApiToken = "",
                WebhookSecret = "",
                WebhookEndpointUrl = "",
                AutoSyncPricing = false,
                MaxParcelWeightKg = 20,
                AllowLiquidCargo = true,
                AllowOversizeCargo = false,
            },
        };

        foreach (var carrier in carriers)
        {
            var existing = await context.CarrierPartners
                .FirstOrDefaultAsync(c => c.CarrierCode == carrier.CarrierCode, cancellationToken)
                .ConfigureAwait(false);

            if (existing == null)
            {
                await context.CarrierPartners.AddAsync(carrier, cancellationToken).ConfigureAwait(false);
            }
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
