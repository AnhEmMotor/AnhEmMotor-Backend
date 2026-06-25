using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface ISupportRequestRepository
{
    public Task<SupportRequest?> GetByIdAsync(int id, CancellationToken cancellationToken);

    public Task<List<SupportRequest>> GetAllAsync(CancellationToken cancellationToken);

    public Task AddAsync(SupportRequest entity, CancellationToken cancellationToken);

    public Task UpdateAsync(SupportRequest entity, CancellationToken cancellationToken);
}
