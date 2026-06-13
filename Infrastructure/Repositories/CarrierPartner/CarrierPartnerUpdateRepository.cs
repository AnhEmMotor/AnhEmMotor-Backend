using Application.Interfaces.Repositories.CarrierPartner;
using Infrastructure.DBContexts;
using CarrierPartnerEntity = Domain.Entities.Logistics.CarrierPartner;

namespace Infrastructure.Repositories.CarrierPartner;

public class CarrierPartnerUpdateRepository(ApplicationDBContext context) : ICarrierPartnerUpdateRepository
{
    public void Update(CarrierPartnerEntity carrierPartner) => context.Set<CarrierPartnerEntity>()
        .Update(carrierPartner);
}
