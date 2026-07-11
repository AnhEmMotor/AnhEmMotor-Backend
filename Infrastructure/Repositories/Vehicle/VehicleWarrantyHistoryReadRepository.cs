using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Vehicle;

public class VehicleWarrantyHistoryReadRepository(ApplicationDBContext context) : IVehicleWarrantyHistoryReadRepository
{
    public Task<List<VehicleWarrantyHistory>> GetByVehicleIdAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        return context.VehicleWarrantyHistories
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(cancellationToken);
    }
}
