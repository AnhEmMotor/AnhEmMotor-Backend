using AnhEmMotor.Application.Interfaces.Repositories.Vehicle;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AnhEmMotor.Infrastructure.Persistence.Repositories.Vehicle
{
    public class VehicleUpdateRepository : IVehicleUpdateRepository
    {
        private readonly ApplicationDBContext _db;
        public VehicleUpdateRepository(ApplicationDBContext db) => _db = db;

        public async Task<bool> UpdateOdoAsync(int vehicleId, double newOdo, CancellationToken cancellationToken)
        {
            var vehicle = await _db.Vehicles.FindAsync(new object[] { vehicleId }, cancellationToken);
            if (vehicle == null) return false;
            vehicle.CurrentOdo = newOdo;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
