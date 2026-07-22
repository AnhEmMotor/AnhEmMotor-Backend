using Domain.Entities;

namespace Application.Interfaces.Repositories.WarrantyTerm;

public interface IWarrantyTermReadRepository
{
    Task<List<Domain.Entities.WarrantyTerm>> GetAllAsync(CancellationToken cancellationToken);
    Task<Domain.Entities.WarrantyTerm?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
