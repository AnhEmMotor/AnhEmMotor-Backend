using Application.Interfaces.Repositories.CarrierPartner;
using CarrierPartnerEntity = Domain.Entities.Logistics.CarrierPartner;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.CarrierPartner;

public class CarrierPartnerUpdateRepository(ApplicationDBContext context) : ICarrierPartnerUpdateRepository
{
    public void Update(CarrierPartnerEntity carrierPartner) => context.Set<CarrierPartnerEntity>().Update(carrierPartner);
}
