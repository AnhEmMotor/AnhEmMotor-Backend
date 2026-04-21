using Domain.Entities;

namespace Application.Interfaces.Repositories.Lead;

public interface ILeadReadRepository
{
    Task<Domain.Entities.Lead?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Lead?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Lead>> GetAllAsync(CancellationToken cancellationToken = default);
    IQueryable<Domain.Entities.Lead> GetQueryable();
}
