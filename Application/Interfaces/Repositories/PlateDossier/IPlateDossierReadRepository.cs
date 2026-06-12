using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.PlateDossier
{
    public interface IPlateDossierReadRepository
    {
public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.PlateDossier, bool>>? filter = null,
            CancellationToken cancellationToken = default);

public Task<Domain.Entities.PlateDossier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        
public Task<Domain.Entities.PlateDossier?> GetByOutputIdAsync(int outputId, CancellationToken cancellationToken = default);
    }
}
