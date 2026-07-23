
namespace Application.Interfaces.Repositories.WarrantyTerm;

public interface IWarrantyTermReadRepository
{
    public Task<List<Domain.Entities.WarrantyTerm>> GetAllAsync(CancellationToken cancellationToken);

    public Task<Domain.Entities.WarrantyTerm?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
