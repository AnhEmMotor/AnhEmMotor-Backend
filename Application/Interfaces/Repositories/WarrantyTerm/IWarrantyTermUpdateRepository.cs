using Domain.Entities;

namespace Application.Interfaces.Repositories.WarrantyTerm;

public interface IWarrantyTermUpdateRepository
{
    void Update(Domain.Entities.WarrantyTerm warrantyTerm);
}
