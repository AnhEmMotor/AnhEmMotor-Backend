using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.MaintenanceHistory
{
    public interface IMaintenanceHistoryReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.MaintenanceHistory, bool>>? filter = null,
            CancellationToken cancellationToken = default);

        public Task<Domain.Entities.MaintenanceHistory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
