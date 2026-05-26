using AnhEmMotor.Application.Interfaces.Repositories.Vehicle;
using AnhEmMotor.Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VehicleEntity = AnhEmMotor.Domain.Entities.Vehicle;

namespace AnhEmMotor.Infrastructure.Persistence.Repositories.Vehicle
{
    public class VehicleReadRepository : IVehicleReadRepository
    {
        private readonly ApplicationDBContext _db;
        public VehicleReadRepository(ApplicationDBContext db) => _db = db;

        public async Task<List<VehicleEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _db.Vehicles.Where(v => v.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<VehicleEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _db.Vehicles.Include(v => v.Product).FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        }
    }
}
