
namespace Application.Interfaces.Repositories.Lead.Lead;

public interface ILeadUpdateRepository
{
    public void Update(Domain.Entities.Lead LeadEntity);

    public Task UpdateAsync(Domain.Entities.Lead LeadEntity, CancellationToken cancellationToken = default);

    public void Restore(Domain.Entities.Lead LeadEntity);
}
