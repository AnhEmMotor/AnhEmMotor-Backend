using CarrierPartnerEntity = Domain.Entities.Logistics.CarrierPartner;

namespace Application.Interfaces.Repositories.CarrierPartner;

public interface ICarrierPartnerReadRepository
{
    Task<List<CarrierPartnerEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CarrierPartnerEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
