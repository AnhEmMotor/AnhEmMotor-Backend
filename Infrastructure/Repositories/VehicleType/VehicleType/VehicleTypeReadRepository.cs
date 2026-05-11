using Application.Common.Models;
using Application.Interfaces.Repositories.VehicleType.VehicleType;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using Application.Interfaces.Repositories;

namespace Infrastructure.Repositories.VehicleType.VehicleType;

public class VehicleTypeReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IVehicleTypeReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.VehicleType, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.GetQuery<Domain.Entities.VehicleType>(mode);
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<Domain.Entities.VehicleType, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<Domain.Entities.VehicleType>(mode)
            .AnyAsync(c => string.Compare(c.Name, name) == 0, cancellationToken);
    }

    public Task<bool> ExistsByNameExceptIdAsync(
        string name,
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<Domain.Entities.VehicleType>(mode)
            .AnyAsync(x => string.Compare(x.Name, name) == 0 && x.Id != id, cancellationToken);
    }

    public Task<List<Domain.Entities.VehicleType>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<Domain.Entities.VehicleType>(mode).ToListAsync(cancellationToken);
    }

    public Task<Domain.Entities.VehicleType?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<Domain.Entities.VehicleType>(mode).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
