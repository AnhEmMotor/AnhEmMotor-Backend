using LeadEntity = AnhEmMotor.Domain.Entities.Lead;
using System.Threading;
using System.Threading.Tasks;

namespace AnhEmMotor.Application.Interfaces.Repositories.Lead
{
    public interface ILeadInsertRepository
    {
        public Task<int> AddAsync(LeadEntity lead, CancellationToken cancellationToken);
    }
}
