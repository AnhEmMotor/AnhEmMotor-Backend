namespace Application.Interfaces.Repositories.Vehicle
{
    public interface IVehicleUpdateRepository
    {
        public void Add(Domain.Entities.Vehicle vehicle);

        public void Update(Domain.Entities.Vehicle vehicle);

        public void Remove(Domain.Entities.Vehicle vehicle);
    }
}

namespace AnhEmMotor.Application.Interfaces.Repositories.Vehicle
{
    public interface IVehicleUpdateRepository
    {
        public Task<bool> UpdateOdoAsync(int vehicleId, double newOdo, CancellationToken cancellationToken);
    }
}
