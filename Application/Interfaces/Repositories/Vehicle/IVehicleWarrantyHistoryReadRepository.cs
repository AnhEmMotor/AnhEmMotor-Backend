using Domain.Entities;

namespace Application.Interfaces.Repositories.Vehicle;

public interface IVehicleWarrantyHistoryReadRepository
{
    Task<List<VehicleWarrantyHistory>> GetByVehicleIdAsync(int vehicleId, CancellationToken cancellationToken = default);
}
