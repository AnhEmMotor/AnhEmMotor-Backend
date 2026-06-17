using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnhEmMotor.Application.Interfaces.Repositories.ServiceBooking
{
    public interface IServiceBookingUpdateRepository
    {
        public Task<bool> UpdateStatusAsync(int id, string status, string reason, DateTime? cancelledAt, CancellationToken cancellationToken);
    }
}
