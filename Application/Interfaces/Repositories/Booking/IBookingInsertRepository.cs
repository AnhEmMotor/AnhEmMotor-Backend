
namespace Application.Interfaces.Repositories.Booking;

public interface IBookingInsertRepository
{
    public void Add(Domain.Entities.Booking booking);

    public void Update(Domain.Entities.Booking booking);
}
