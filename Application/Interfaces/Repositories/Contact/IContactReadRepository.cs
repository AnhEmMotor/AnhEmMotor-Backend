using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface IContactReadRepository
{
    public Task<Domain.Entities.Contact?> GetByIdAsync(int id, CancellationToken cancellationToken);
    public Task<List<Domain.Entities.Contact>> GetAllAsync(CancellationToken cancellationToken);
}
