using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface IContactReadRepository
{
    Task<Domain.Entities.Contact?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Domain.Entities.Contact>> GetAllAsync(CancellationToken cancellationToken);
}
