using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories.CarrierPartner;
using Application.ApiContracts.Logistics.CarrierSettings.Requests;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Commands.UpdateCarrierPartner;

public class UpdateCarrierPartnerCommandHandler(
    ICarrierPartnerReadRepository carrierPartnerReadRepository,
    ICarrierPartnerUpdateRepository carrierPartnerUpdateRepository,
    Application.Interfaces.Repositories.IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCarrierPartnerCommand, bool>
{
    public async Task<bool> Handle(UpdateCarrierPartnerCommand request, CancellationToken cancellationToken)
    {
        var entity = await carrierPartnerReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) return false;

        Apply(entity, request.Request);
        carrierPartnerUpdateRepository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Apply(CarrierPartner entity, UpdateCarrierPartnerRequest req)
    {
        entity.IsActive = req.IsActive;
        entity.Environment = req.Environment;
        entity.ApiBaseUrl = req.ApiBaseUrl;
        entity.WebhookEndpointUrl = req.WebhookEndpointUrl;
        entity.AutoSyncPricing = req.AutoSyncPricing;
        entity.MaxParcelWeightKg = req.MaxParcelWeightKg;
        entity.AllowLiquidCargo = req.AllowLiquidCargo;
        entity.AllowOversizeCargo = req.AllowOversizeCargo;

        // Only update secrets if provided (UI sends plain when user clicks show/edit; here accept optional)
        if (!string.IsNullOrWhiteSpace(req.ApiTokenPlain))
            entity.ApiToken = req.ApiTokenPlain!;
        if (!string.IsNullOrWhiteSpace(req.WebhookSecretPlain))
            entity.WebhookSecret = req.WebhookSecretPlain!;
    }
}

