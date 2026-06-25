using CarrierPartnerEntity = Domain.Entities.Logistics.CarrierPartner;

namespace Application.Interfaces.Repositories.CarrierPartner;

public interface ICarrierPartnerReadRepository
{
    public Task<List<CarrierPartnerEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<CarrierPartnerEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

