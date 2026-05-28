using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Infrastructure.Repositories.PurchaseRequest
{
    public class PurchaseRequestReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IPurchaseRequestReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            return paginator.ApplyAsync<PurchaseRequestEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        public Task<PurchaseRequestEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode);
            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<PurchaseRequestEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode)
                .Include(x => x.CreatedByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.PurchaseRequestItems)
                    .ThenInclude(r => r.ProductVariant)
                    .ThenInclude(pv => pv!.Product)
                .Include(x => x.PurchaseRequestItems)
                    .ThenInclude(r => r.ProductVariantColor)
                .Include(x => x.PurchaseRequestItems)
                    .ThenInclude(r => r.InputInfos)
                        .ThenInclude(ii => ii.InputReceipt)
                .AsSplitQuery();

            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        private IQueryable<PurchaseRequestEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.PurchaseRequests.IgnoreQueryFilters();
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(x => x.DeletedAt == null);
            }
            else if (mode == DataFetchMode.DeletedOnly)
            {
                query = query.Where(x => x.DeletedAt != null);
            }
            return query;
        }

        public Task<List<Domain.Entities.PurchaseRequestItem>> GetItemsByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken)
        {
            return context.PurchaseRequestItems
                .Include(x => x.ProductVariant)
                    .ThenInclude(pv => pv!.Product)
                .Include(x => x.ProductVariantColor)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
