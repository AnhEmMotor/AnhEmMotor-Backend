using Application.Interfaces.Repositories.ServiceBooking;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.ServiceBooking;

public class ServiceBookingReadRepository(ApplicationDBContext context) : IServiceBookingReadRepository
{
    public Task<Domain.Entities.ServiceBooking?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.ServiceBookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public Task<List<Domain.Entities.ServiceBooking>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.ServiceBookings.ToListAsync(cancellationToken);
    }

    public Task<List<Domain.Entities.ServiceBooking>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return context.ServiceBookings.ToListAsync(cancellationToken);
    }
}
