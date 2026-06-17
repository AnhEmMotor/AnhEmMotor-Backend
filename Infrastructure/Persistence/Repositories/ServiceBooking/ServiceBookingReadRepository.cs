using AnhEmMotor.Application.Interfaces.Repositories.ServiceBooking;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ServiceBookingEntity = AnhEmMotor.Domain.Entities.ServiceBooking;

namespace AnhEmMotor.Infrastructure.Persistence.Repositories.ServiceBooking
{
    public class ServiceBookingReadRepository : IServiceBookingReadRepository
    {
        private readonly ApplicationDBContext _db;
        public ServiceBookingReadRepository(ApplicationDBContext db) => _db = db;

        public async Task<ServiceBookingEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _db.ServiceBookings
                .Include(b => b.Vehicle)
                .Include(b => b.AssignedSale)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<List<ServiceBookingEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _db.ServiceBookings.ToListAsync(cancellationToken);
        }

        public async Task<List<ServiceBookingEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                return [];
            }

            return await _db.ServiceBookings
                .Where(b => b.Vehicle.UserId == parsedUserId)
                .ToListAsync(cancellationToken);
        }
    }
}
