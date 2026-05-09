using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Vehicle;

public class VehicleReadRepository(ApplicationDBContext context) : IVehicleReadRepository
{
    public IQueryable<Domain.Entities.Vehicle> GetQuery(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<Domain.Entities.Vehicle>(mode);
    }

    public IQueryable<Domain.Entities.Vehicle> All()
    {
        return context.All<Domain.Entities.Vehicle>();
    }

    public async Task<List<Domain.Entities.Vehicle>> GetVehiclesAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = GetQuery(DataFetchMode.ActiveOnly).Include(v => v.Lead).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(
                v => v.LicensePlate.Contains(search) || v.VinNumber.Contains(search) || v.Lead.FullName.Contains(search));
        }
        return query
            .OrderByDescending(v => v.PurchaseDate)
            .ToListAsync(cancellationToken);
    }
}
