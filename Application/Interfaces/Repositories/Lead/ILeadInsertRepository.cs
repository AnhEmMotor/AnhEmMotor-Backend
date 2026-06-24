using LeadEntity = global::Domain.Entities.Lead;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.Lead
{
    public interface ILeadInsertRepository
    {
        public Task<int> AddAsync(LeadEntity lead, CancellationToken cancellationToken);
    }
}
