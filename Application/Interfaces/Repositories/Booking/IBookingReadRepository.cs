using Domain.Entities;

namespace Application.Interfaces.Repositories.Booking;

public interface IBookingReadRepository
{
    public Task<Domain.Entities.Booking?> GetByIdAsync(int id, CancellationToken cancellationToken);
    public Task<List<Domain.Entities.Booking>> GetAllAsync(CancellationToken cancellationToken);
}
