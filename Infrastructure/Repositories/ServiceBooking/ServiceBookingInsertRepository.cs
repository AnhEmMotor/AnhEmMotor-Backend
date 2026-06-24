using Application.Interfaces.Repositories.ServiceBooking;
using Infrastructure.DBContexts;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.ServiceBooking;

public class ServiceBookingInsertRepository(ApplicationDBContext context) : IServiceBookingInsertRepository
{
    public async Task<int> AddAsync(Domain.Entities.ServiceBooking booking, CancellationToken cancellationToken)
    {
        context.ServiceBookings.Add(booking);
        await context.SaveChangesAsync(cancellationToken);
        return booking.Id;
    }
}
