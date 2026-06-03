using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.RepairOrder;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.RepairOrder
{
    public class RepairOrderReadRepository(
        ApplicationDBContext context,
        ISievePaginator paginator) : IRepairOrderReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.RepairOrder, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<Domain.Entities.RepairOrder, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        public Task<Domain.Entities.RepairOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return context.RepairOrders
                .Include(r => r.Vehicle)
                .Include(r => r.Technician)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Service)
                .Include(r => r.Details)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public Task<List<Domain.Entities.RepairOrder>> GetByCustomerPhoneAsync(string phone, CancellationToken cancellationToken = default)
        {
            return context.RepairOrders
                .Include(r => r.Vehicle)
                .Include(r => r.Technician)
                .Include(r => r.Details)
                    .ThenInclude(d => d.Service)
                .Include(r => r.Details)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                .Where(r => r.CustomerPhone == phone)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        private IQueryable<Domain.Entities.RepairOrder> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<Domain.Entities.RepairOrder>(mode)
                .Include(r => r.Vehicle)
                .Include(r => r.Technician);
        }
    }
}
