using AnhEmMotor.Application.Interfaces.Repositories.ServiceBooking;
using Infrastructure.DBContexts;
using System.Threading;
using System.Threading.Tasks;
using ServiceBookingEntity = AnhEmMotor.Domain.Entities.ServiceBooking;

namespace AnhEmMotor.Infrastructure.Persistence.Repositories.ServiceBooking
{
    public class ServiceBookingInsertRepository : IServiceBookingInsertRepository
    {
        private readonly ApplicationDBContext _db;
        public ServiceBookingInsertRepository(ApplicationDBContext db) => _db = db;

        public async Task<int> AddAsync(ServiceBookingEntity booking, CancellationToken cancellationToken)
        {
            _db.ServiceBookings.Add(booking);
            await _db.SaveChangesAsync(cancellationToken);
            return booking.Id;
        }
    }
}
