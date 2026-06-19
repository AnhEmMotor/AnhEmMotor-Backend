using Application.Interfaces.Repositories.Vehicle;
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

    public Task InsertAuditLogsAsync(IEnumerable<Domain.Entities.VehicleAuditLog> logs, CancellationToken ct = default)
    {
        dbContext.VehicleAuditLogs.AddRange(logs);
        return Task.CompletedTask;
    }
}
