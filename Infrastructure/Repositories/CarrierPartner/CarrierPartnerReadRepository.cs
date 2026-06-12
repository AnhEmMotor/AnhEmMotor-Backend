using Application.Interfaces.Repositories.CarrierPartner;
using CarrierPartnerEntity = Domain.Entities.Logistics.CarrierPartner;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.CarrierPartner;

public class CarrierPartnerReadRepository(ApplicationDBContext context) : ICarrierPartnerReadRepository
{
    public Task<List<CarrierPartnerEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => context.Set<CarrierPartnerEntity>().OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public Task<CarrierPartnerEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.Set<CarrierPartnerEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
