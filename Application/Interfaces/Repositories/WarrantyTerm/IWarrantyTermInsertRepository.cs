using Domain.Entities;

namespace Application.Interfaces.Repositories.WarrantyTerm;

public interface IWarrantyTermInsertRepository
{
    void Add(Domain.Entities.WarrantyTerm warrantyTerm);
}
