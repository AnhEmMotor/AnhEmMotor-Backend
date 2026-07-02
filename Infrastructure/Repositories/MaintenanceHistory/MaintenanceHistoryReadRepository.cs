using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MaintenanceHistory;
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

namespace Infrastructure.Repositories.MaintenanceHistory
{
    public class MaintenanceHistoryReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IMaintenanceHistoryReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.MaintenanceHistory, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<Domain.Entities.MaintenanceHistory, TResponse>(
                query,
                sieveModel,
                mode,
                cancellationToken);
        }

        public Task<Domain.Entities.MaintenanceHistory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return context.MaintenanceHistories
                .Include(m => m.Vehicle)
                    .ThenInclude(v => v.User)
                .Include(m => m.Vehicle)
                    .ThenInclude(v => v.ProductVariantColor)
                .Include(m => m.Vehicle)
                    .ThenInclude(v => v.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                            .ThenInclude(p => p!.Brand)
                .Include(m => m.Technician)
                    .ThenInclude(t => t!.User)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        private IQueryable<Domain.Entities.MaintenanceHistory> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<Domain.Entities.MaintenanceHistory>(mode)
                .Include(m => m.Vehicle)
                    .ThenInclude(v => v.User);
        }
    }
}
