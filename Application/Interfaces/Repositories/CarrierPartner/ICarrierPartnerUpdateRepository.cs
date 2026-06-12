using CarrierPartnerEntity = Domain.Entities.Logistics.CarrierPartner;

namespace Application.Interfaces.Repositories.CarrierPartner;

public interface ICarrierPartnerUpdateRepository
{
    void Update(CarrierPartnerEntity carrierPartner);
}
