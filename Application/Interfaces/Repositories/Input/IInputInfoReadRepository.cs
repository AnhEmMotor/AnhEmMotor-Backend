using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.Input
{
    public interface IInputInfoReadRepository
    {
public Task<List<InputInfo>> GetFinishedInputInfosByVariantIdAsync(int variantId, CancellationToken cancellationToken = default);
    }
}
