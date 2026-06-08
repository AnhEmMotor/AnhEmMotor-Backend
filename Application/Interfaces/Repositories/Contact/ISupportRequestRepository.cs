using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface ISupportRequestRepository
{
    Task<SupportRequest?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<SupportRequest>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(SupportRequest entity, CancellationToken cancellationToken);
    Task UpdateAsync(SupportRequest entity, CancellationToken cancellationToken);
}
