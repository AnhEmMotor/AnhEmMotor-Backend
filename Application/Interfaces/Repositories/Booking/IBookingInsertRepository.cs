using Domain.Entities;

namespace Application.Interfaces.Repositories.Booking;

public interface IBookingInsertRepository
{
    void Add(Domain.Entities.Booking booking);
    void Update(Domain.Entities.Booking booking);
}
