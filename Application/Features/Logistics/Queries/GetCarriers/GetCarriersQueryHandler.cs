using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.ApiContracts.Logistics.CarrierSettings.Responses;
using Application.Interfaces.Repositories.CarrierPartner;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Queries.GetCarriers;

public class GetCarriersQueryHandler(ICarrierPartnerReadRepository carrierPartnerReadRepository)
    : IRequestHandler<GetCarriersQuery, GetCarriersResponse>
{
    public async Task<GetCarriersResponse> Handle(GetCarriersQuery request, CancellationToken cancellationToken)
    {
        // Note: async EF is available; using ToList + Task.FromResult for consistency.
        var items = (await carrierPartnerReadRepository.GetAllAsync(cancellationToken))
            .Select(x => new CarrierPartnerDto
            {
                Id = x.Id,
                CarrierCode = x.CarrierCode,
                Name = x.Name,
                IsActive = x.IsActive,
                Environment = x.Environment,
                ApiBaseUrl = x.ApiBaseUrl,
                ApiTokenMasked = MaskSecret(x.ApiToken),
                ApiTokenPlain = x.ApiToken,
                WebhookSecretMasked = MaskSecret(x.WebhookSecret),
                WebhookSecretPlain = x.WebhookSecret,
                WebhookEndpointUrl = x.WebhookEndpointUrl,
                AutoSyncPricing = x.AutoSyncPricing,
                MaxParcelWeightKg = x.MaxParcelWeightKg,
                AllowLiquidCargo = x.AllowLiquidCargo,
                AllowOversizeCargo = x.AllowOversizeCargo,
            })
            .ToList();

        return await Task.FromResult(new GetCarriersResponse { Items = items });
    }

    private static string MaskSecret(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var s = value.Trim();
        if (s.Length <= 6) return new string('•', s.Length);
        return new string('•', 10);
    }
}

