using ServiceBookingEntity = Domain.Entities.ServiceBooking;

namespace Application.Interfaces.Repositories.ServiceBooking
{
    public interface IServiceBookingReadRepository
    {
        public Task<ServiceBookingEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<List<ServiceBookingEntity>> GetAllAsync(CancellationToken cancellationToken);

        public Task<List<ServiceBookingEntity>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    }
}
