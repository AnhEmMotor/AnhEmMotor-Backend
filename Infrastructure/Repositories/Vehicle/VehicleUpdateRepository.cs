using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Vehicle;

public class VehicleUpdateRepository(ApplicationDBContext dbContext) : IVehicleUpdateRepository
{
    public void Add(Domain.Entities.Vehicle vehicle)
    {
        dbContext.Vehicles.Add(vehicle);
    }

    public void Update(Domain.Entities.Vehicle vehicle)
    {
        dbContext.Vehicles.Update(vehicle);
    }

    public void Remove(Domain.Entities.Vehicle vehicle)
    {
        dbContext.Vehicles.Remove(vehicle);
    }

    public Task InsertAuditLogsAsync(IEnumerable<VehicleAuditLog> logs, CancellationToken ct = default)
    {
        dbContext.VehicleAuditLogs.AddRange(logs);
        return Task.CompletedTask;
    }

    public async Task UpdateOdoAsync(int vehicleId, double newOdo, CancellationToken cancellationToken = default)
    {
        var vehicle = await dbContext.Vehicles.FindAsync(new object[] { vehicleId }, cancellationToken);
        if (vehicle != null)
        {
            vehicle.CurrentOdo = newOdo;
            dbContext.Vehicles.Update(vehicle);
        }
    }
}
