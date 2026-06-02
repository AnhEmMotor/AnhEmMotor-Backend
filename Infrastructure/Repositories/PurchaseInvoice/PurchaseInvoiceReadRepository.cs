using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Infrastructure.Repositories.PurchaseInvoice
{
    public class PurchaseInvoiceReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IPurchaseInvoiceReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode)
                .Include(x => x.CreatedByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.PurchaseOrder)
                    .ThenInclude(po => po!.Supplier)
                .Include(x => x.PurchaseInvoiceItems.Where(pii => pii.DeletedAt == null));

            return paginator.ApplyAsync<PurchaseInvoiceEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        public Task<PurchaseInvoiceEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode);
            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<PurchaseInvoiceEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode)
                .Include(x => x.CreatedByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.PurchaseOrder)
                    .ThenInclude(po => po!.Supplier)
                .Include(x => x.PurchaseInvoiceItems.Where(pii => pii.DeletedAt == null))
                    .ThenInclude(pii => pii.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                .Include(x => x.PurchaseInvoiceItems.Where(pii => pii.DeletedAt == null))
                    .ThenInclude(pii => pii.ProductVariantColor)
                .AsSplitQuery();

            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        private IQueryable<PurchaseInvoiceEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.PurchaseInvoices.IgnoreQueryFilters();
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
                .Include(x => x.ApprovedByUser);
        }
    }
}
