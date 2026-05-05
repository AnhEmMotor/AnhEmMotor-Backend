using Application.Interfaces.Repositories.Booking;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Booking;

public class BookingReadRepository(ApplicationDBContext context) : IBookingReadRepository
{
    public Task<Domain.Entities.Booking?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Bookings
            .Include(b => b.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public Task<List<Domain.Entities.Booking>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.Bookings
            .Include(b => b.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .OrderByDescending(b => b.PreferredDate)
            .ToListAsync(cancellationToken);
    }
}
