using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.Lead;

public interface ILeadWriteRepository
{
    public void Add(Domain.Entities.Lead lead);
    public void Update(Domain.Entities.Lead lead);
    public Task UpdateAsync(Domain.Entities.Lead lead, CancellationToken cancellationToken = default);
    public Task ClearAllAsync(CancellationToken cancellationToken = default);
}
