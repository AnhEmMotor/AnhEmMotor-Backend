using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Quotation;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq;
using QuotationEntity = Domain.Entities.Quotation;

namespace Infrastructure.Repositories.Quotation
{
    public class QuotationReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IQuotationReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            return paginator.ApplyAsync<QuotationEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        public Task<QuotationEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode);
            return query
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<QuotationEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode)
                .Include(x => x.Supplier)
                .Include(x => x.QuotationProductRows)
                .ThenInclude(r => r.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .Include(x => x.QuotationProductRows)
                .ThenInclude(r => r.ProductVariantColor)
                .AsSplitQuery();
            return query
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        private IQueryable<QuotationEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.Quotations.IgnoreQueryFilters();
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(x => x.DeletedAt == null);
            } else if (mode == DataFetchMode.DeletedOnly)
            {
                query = query.Where(x => x.DeletedAt != null);
            }
            return query
                .Include(x => x.Supplier)
                .Include(x => x.QuotationProductRows);
        }

        public Task<List<QuotationProductRow>> GetApprovedQuotationRowsByVariantsAsync(
            IEnumerable<int> variantIds,
            CancellationToken cancellationToken)
        {
            return context.QuotationProductRows
                .Include(r => r.QuotationReceipt)
                .ThenInclude(q => q!.Supplier)
                .Where(
                    r => r.QuotationReceipt != null &&
                        r.QuotationReceipt.Status == "approved" &&
                        r.ProductVariantId != null &&
                        variantIds.Contains(r.ProductVariantId.Value))
                .ToListAsync(cancellationToken);
        }

        public Task<List<QuotationProductRow>> GetRowsByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken)
        {
            return context.QuotationProductRows
                .Include(x => x.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .Include(x => x.ProductVariantColor)
                .Include(x => x.QuotationReceipt)
                .ThenInclude(q => q!.Supplier)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
