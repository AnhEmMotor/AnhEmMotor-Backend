namespace Application.Interfaces.Repositories.Vehicle
{
    public interface IVehicleUpdateRepository
    {
        public void Add(Domain.Entities.Vehicle vehicle);

        public void Update(Domain.Entities.Vehicle vehicle);

    public void Remove(Domain.Entities.Vehicle vehicle);

    public Task InsertAuditLogsAsync(IEnumerable<Domain.Entities.VehicleAuditLog> logs, CancellationToken ct = default);

    public Task UpdateOdoAsync(int vehicleId, double newOdo, CancellationToken cancellationToken = default);
    }
}
