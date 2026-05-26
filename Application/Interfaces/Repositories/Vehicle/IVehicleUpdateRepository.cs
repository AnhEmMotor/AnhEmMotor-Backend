using System.Threading;
using System.Threading.Tasks;

namespace AnhEmMotor.Application.Interfaces.Repositories.Vehicle
{
    public interface IVehicleUpdateRepository
    {
        public Task<bool> UpdateOdoAsync(int vehicleId, double newOdo, CancellationToken cancellationToken);
    }
}