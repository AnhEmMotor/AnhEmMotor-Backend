using Domain.Entities;

namespace Application.Interfaces.Repositories.Vehicle;

public interface IVehiclePurchaseHistoryReadRepository
{
    Task<List<VehiclePurchaseHistory>> GetByVehicleIdAsync(int vehicleId, CancellationToken cancellationToken = default);
}
