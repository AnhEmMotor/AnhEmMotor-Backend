using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseOrder;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;

namespace Infrastructure.Repositories.PurchaseOrder
{
    public class PurchaseOrderReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IPurchaseOrderReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode)
                .Include(x => x.CreatedByUser)
                .Include(x => x.PurchaseOrderItems);
            return paginator.ApplyAsync<PurchaseOrderEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        public Task<PurchaseOrderEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode);
            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<PurchaseOrderEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode)
                .Include(x => x.CreatedByUser)
                .Include(x => x.SentByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser)
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseRequest)
                .Include(x => x.PurchaseOrderItems)
                .ThenInclude(r => r.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .ThenInclude(p => p!.ProductCategory)
                .Include(x => x.PurchaseOrderItems)
                .ThenInclude(r => r.ProductVariantColor)
                .AsSplitQuery();
            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        private IQueryable<PurchaseOrderEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.PurchaseOrders.IgnoreQueryFilters();
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(x => x.DeletedAt == null);
            }
            else if (mode == DataFetchMode.DeletedOnly)
            {
                query = query.Where(x => x.DeletedAt != null);
            }
            return query
                .Include(x => x.CreatedByUser)
                .Include(x => x.SentByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser);
        }
    }
}
