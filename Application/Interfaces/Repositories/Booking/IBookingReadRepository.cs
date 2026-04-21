using Domain.Entities;

namespace Application.Interfaces.Repositories.Booking;

public interface IBookingReadRepository
{
    Task<Domain.Entities.Booking?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Domain.Entities.Booking>> GetAllAsync(CancellationToken cancellationToken);
}
