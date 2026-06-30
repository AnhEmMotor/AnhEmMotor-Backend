using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyClaim;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.WarrantyClaim
{
    public class WarrantyClaimReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IWarrantyClaimReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.WarrantyClaim, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<Domain.Entities.WarrantyClaim, TResponse>(
                query,
                sieveModel,
                mode,
                cancellationToken);
        }

        public Task<Domain.Entities.WarrantyClaim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return context.WarrantyClaims
                .Include(w => w.Parts)
                .Include(w => w.Vehicle)
                    .ThenInclude(v => v!.User)
                .Include(w => w.Vehicle)
                    .ThenInclude(v => v!.ProductVariantColor)
                .Include(w => w.Vehicle)
                    .ThenInclude(v => v!.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                            .ThenInclude(p => p!.Brand)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        private IQueryable<Domain.Entities.WarrantyClaim> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<Domain.Entities.WarrantyClaim>(mode)
                .Include(w => w.Vehicle)
                    .ThenInclude(v => v!.User)
                .Include(w => w.Parts);
        }
    }
}
