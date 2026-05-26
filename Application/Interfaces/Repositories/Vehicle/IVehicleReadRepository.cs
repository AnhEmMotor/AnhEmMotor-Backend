using AnhEmMotor.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VehicleEntity = AnhEmMotor.Domain.Entities.Vehicle;

namespace AnhEmMotor.Application.Interfaces.Repositories.Vehicle
{
    public interface IVehicleReadRepository
    {
        public Task<List<VehicleEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
        public Task<VehicleEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);
    }
}
