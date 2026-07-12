using Application.Interfaces.Repositories.WarrantyClaim;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.WarrantyClaim;

public class WarrantyClaimWriteRepository(ApplicationDBContext context) : IWarrantyClaimWriteRepository
{
    public void Add(global::Domain.Entities.WarrantyClaim entity) => context.Set<global::Domain.Entities.WarrantyClaim>(
        )
        .Add(entity);

    public void AddPart(global::Domain.Entities.WarrantyClaimPart part) => context.Set<global::Domain.Entities.WarrantyClaimPart>(
        )
        .Add(part);

    public void Update(global::Domain.Entities.WarrantyClaim entity) => context.Set<global::Domain.Entities.WarrantyClaim>(
        )
        .Update(entity);

    public void Delete(global::Domain.Entities.WarrantyClaim entity) => context.Set<global::Domain.Entities.WarrantyClaim>(
        )
        .Remove(entity);
}
