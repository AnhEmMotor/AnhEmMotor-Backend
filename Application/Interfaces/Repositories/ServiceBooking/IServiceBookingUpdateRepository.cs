using System;

namespace Application.Interfaces.Repositories.ServiceBooking
{
    public interface IServiceBookingUpdateRepository
    {
        public Task<bool> UpdateStatusAsync(
            int id,
            string status,
            string reason,
            DateTime? cancelledAt,
            CancellationToken cancellationToken);
    }
}
