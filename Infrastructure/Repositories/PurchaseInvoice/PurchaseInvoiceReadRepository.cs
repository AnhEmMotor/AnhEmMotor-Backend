using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;
using Domain.Constants;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq;

namespace Infrastructure.Repositories.PurchaseInvoice
{
    public class PurchaseInvoiceReadRepository(
        ApplicationDBContext context,
        ISievePaginator paginator) : IPurchaseInvoiceReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default)
        {
            var query = context.Set<PurchaseInvoiceEntity>()
                .IgnoreQueryFilters()
                .Include(x => x.PurchaseInvoiceItems)
                .Include(x => x.Supplier)
                .AsSplitQuery();

            if (mode == DataFetchMode.ActiveOnly)
                query = query.Where(x => x.DeletedAt == null);
            else if (mode == DataFetchMode.DeletedOnly)
                query = query.Where(x => x.DeletedAt != null);

            return paginator.ApplyAsync<PurchaseInvoiceEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        public Task<PurchaseInvoiceEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.Set<PurchaseInvoiceEntity>().IgnoreQueryFilters();
            if (mode == DataFetchMode.ActiveOnly)
                query = query.Where(x => x.DeletedAt == null);
            else if (mode == DataFetchMode.DeletedOnly)
                query = query.Where(x => x.DeletedAt != null);
            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<PurchaseInvoiceEntity?> GetByIdWithItemsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.Set<PurchaseInvoiceEntity>()
                .IgnoreQueryFilters()
                .Include(x => x.PurchaseInvoiceItems)
                .Include(x => x.Supplier)
                .AsSplitQuery();

            if (mode == DataFetchMode.ActiveOnly)
                query = query.Where(x => x.DeletedAt == null);
            else if (mode == DataFetchMode.DeletedOnly)
                query = query.Where(x => x.DeletedAt != null);

            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}
