using AnhEmMotor.Application.Interfaces.Repositories.Vehicle;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VehicleEntity = global::Domain.Entities.Vehicle;

namespace AnhEmMotor.Infrastructure.Persistence.Repositories.Vehicle
{
    public class VehicleReadRepository : IVehicleReadRepository
    {
        private readonly ApplicationDBContext _db;
        public VehicleReadRepository(ApplicationDBContext db) => _db = db;

        public async Task<List<VehicleEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                return [];
            }

            return await _db.Vehicles
                .Include(v => v.Product)
                .Where(v => v.UserId == parsedUserId)
                .ToListAsync(cancellationToken);
        }

        public async Task<VehicleEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _db.Vehicles.Include(v => v.Product).FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        }
    }
}
