using Application.Interfaces.Repositories.Vehicle;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Vehicle;

public class VehicleReadRepository(ApplicationDBContext context) : IVehicleReadRepository
{
    public async Task<List<Domain.Entities.Vehicle>> GetVehiclesAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = context.Vehicles
            .Include(v => v.Lead)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.LicensePlate.Contains(search) || 
                                     v.VinNumber.Contains(search) ||
                                     v.Lead.FullName.Contains(search));
        }

        return await query
            .OrderByDescending(v => v.PurchaseDate)
            .ToListAsync(cancellationToken);
    }
}
