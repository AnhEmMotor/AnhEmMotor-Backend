using global::Domain.Entities;

namespace Application.Interfaces.Repositories.WarrantyClaim;

public interface IWarrantyClaimWriteRepository
{
    void Add(global::Domain.Entities.WarrantyClaim entity);

    void Update(global::Domain.Entities.WarrantyClaim entity);

    void Delete(global::Domain.Entities.WarrantyClaim entity);

    void AddPart(WarrantyClaimPart part);
}
