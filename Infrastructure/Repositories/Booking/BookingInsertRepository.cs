using Application.Interfaces.Repositories.Booking;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Booking;

public class BookingInsertRepository(ApplicationDBContext context) : IBookingInsertRepository
{
    public void Add(Domain.Entities.Booking booking)
    {
        context.Bookings.Add(booking);
    }

    public void Update(Domain.Entities.Booking booking)
    {
        context.Bookings.Update(booking);
    }
}
