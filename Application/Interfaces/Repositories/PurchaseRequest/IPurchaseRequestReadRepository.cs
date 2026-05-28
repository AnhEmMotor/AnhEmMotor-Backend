using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Threading;
using System.Threading.Tasks;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Application.Interfaces.Repositories.PurchaseRequest
{
    public interface IPurchaseRequestReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default);

        public Task<PurchaseRequestEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<PurchaseRequestEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<List<Domain.Entities.PurchaseRequestItem>> GetItemsByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken);
    }
}
