
namespace Application.Interfaces.Repositories.Lead;

public interface ILeadReadRepository
{
    public Task<Domain.Entities.Lead?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<Domain.Entities.Lead?> GetByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Lead>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Lead>> GetAllLeadsWithActivitiesAsync(
        CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Lead>> GetLoyaltyMembersAsync(
        string? search,
        CancellationToken cancellationToken = default);

    public IQueryable<Domain.Entities.Lead> GetQueryable();
}
