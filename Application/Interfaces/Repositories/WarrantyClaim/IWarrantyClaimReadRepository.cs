using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.WarrantyClaim
{
    public interface IWarrantyClaimReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.WarrantyClaim, bool>>? filter = null,
            CancellationToken cancellationToken = default);

        public Task<Domain.Entities.WarrantyClaim?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
