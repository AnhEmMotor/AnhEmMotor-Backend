using LeadEntity = global::Domain.Entities.Lead;

namespace Application.Interfaces.Repositories.Lead
{
    public interface ILeadInsertRepository
    {
        public Task<int> AddAsync(LeadEntity lead, CancellationToken cancellationToken);
    }
}
