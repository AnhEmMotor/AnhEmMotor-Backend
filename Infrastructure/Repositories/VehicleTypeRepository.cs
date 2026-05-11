using Application.Common.Models;
using Domain.Primitives;
using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class VehicleTypeRepository(ApplicationDBContext context, ISievePaginator paginator) : IVehicleTypeRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<VehicleType, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<VehicleType, TResponse>(query, sieveModel, mode, cancellationToken);
        }
        public Task<bool> ExistsByNameAsync(
            string name,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<VehicleType>(mode)
                .AnyAsync(c => string.Compare(c.Name, name) == 0, cancellationToken);
        }

        public Task<bool> ExistsByNameExceptIdAsync(
            string name,
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<VehicleType>(mode)
                .AnyAsync(x => string.Compare(x.Name, name) == 0 && x.Id != id, cancellationToken);
        }

        public Task<List<VehicleType>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<VehicleType>(mode).ToListAsync(cancellationToken);
        }

        public Task<VehicleType?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<VehicleType>(mode).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        internal IQueryable<VehicleType> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<VehicleType>(mode);
        }

        public void Add(VehicleType vehicleType)
        {
            context.VehicleTypes.Add(vehicleType);
        }

        public void Update(VehicleType vehicleType)
        {
            context.VehicleTypes.Update(vehicleType);
        }

        public void Remove(VehicleType vehicleType)
        {
            context.VehicleTypes.Remove(vehicleType);
        }
    }
}
