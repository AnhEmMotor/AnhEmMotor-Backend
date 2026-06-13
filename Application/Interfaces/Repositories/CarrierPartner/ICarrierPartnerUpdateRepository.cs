using CarrierPartnerEntity = Domain.Entities.Logistics.CarrierPartner;

namespace Application.Interfaces.Repositories.CarrierPartner;

public interface ICarrierPartnerUpdateRepository
{
    public void Update(CarrierPartnerEntity carrierPartner);
}

