using AnhEmMotor.Application.Interfaces.Repositories.ServiceBooking;
using AnhEmMotor.Domain.Constants;
using Infrastructure.DBContexts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnhEmMotor.Infrastructure.Persistence.Repositories.ServiceBooking
{
    public class ServiceBookingUpdateRepository : IServiceBookingUpdateRepository
    {
        private readonly ApplicationDBContext _db;
        public ServiceBookingUpdateRepository(ApplicationDBContext db) => _db = db;

        public async Task<bool> UpdateStatusAsync(int id, string status, string reason, DateTime? cancelledAt, CancellationToken cancellationToken)
        {
            var booking = await _db.ServiceBookings.FindAsync(new object[] { id }, cancellationToken);
            if (booking == null) return false;

            booking.Status = Enum.Parse<BookingStatus>(status);
            booking.CancellationReason = reason;
            booking.CancelledAt = cancelledAt;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
