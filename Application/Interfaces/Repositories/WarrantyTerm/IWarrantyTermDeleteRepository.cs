using Domain.Entities;

namespace Application.Interfaces.Repositories.WarrantyTerm;

public interface IWarrantyTermDeleteRepository
{
    void Delete(Domain.Entities.WarrantyTerm warrantyTerm);
}
