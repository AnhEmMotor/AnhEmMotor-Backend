using ServiceBookingEntity = Domain.Entities.ServiceBooking;

namespace Application.Interfaces.Repositories.ServiceBooking
{
    public interface IServiceBookingInsertRepository
    {
        public Task<int> AddAsync(ServiceBookingEntity booking, CancellationToken cancellationToken);
    }
}
