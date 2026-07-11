using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Vehicle;

public class VehiclePurchaseHistoryReadRepository(ApplicationDBContext context) : IVehiclePurchaseHistoryReadRepository
{
    public Task<List<VehiclePurchaseHistory>> GetByVehicleIdAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        return context.VehiclePurchaseHistories
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.PurchaseDate)
            .ToListAsync(cancellationToken);
    }
}
