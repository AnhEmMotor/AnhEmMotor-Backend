using global::Domain.Entities;

namespace Application.Interfaces.Repositories.WarrantyClaim;

public interface IWarrantyClaimWriteRepository
{
    public void Add(global::Domain.Entities.WarrantyClaim entity);

    public void Update(global::Domain.Entities.WarrantyClaim entity);

    public void Delete(global::Domain.Entities.WarrantyClaim entity);

    public void AddPart(WarrantyClaimPart part);
}
