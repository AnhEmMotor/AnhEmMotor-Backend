using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Quotation;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                    .ThenInclude(pv => pv.Product)
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
            }
            else if (mode == DataFetchMode.DeletedOnly)
            {
                query = query.Where(x => x.DeletedAt != null);
            }
            return query
                .Include(x => x.Supplier)
                .Include(x => x.QuotationProductRows);
        }
    }
}
