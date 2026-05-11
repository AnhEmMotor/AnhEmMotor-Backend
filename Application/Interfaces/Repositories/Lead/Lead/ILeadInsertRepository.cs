
namespace Application.Interfaces.Repositories.Lead.Lead;

public interface ILeadInsertRepository
{
    public void Add(Domain.Entities.Lead LeadEntity);

    public Task AddAsync(Domain.Entities.Lead LeadEntity, CancellationToken cancellationToken = default);
}
