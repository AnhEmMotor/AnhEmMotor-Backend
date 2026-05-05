using Application.Interfaces.Repositories.Booking;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Booking;

public class BookingReadRepository(ApplicationDBContext context) : IBookingReadRepository
{
    public async Task<Domain.Entities.Booking?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Bookings
            .Include(b => b.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Domain.Entities.Booking>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Bookings
            .Include(b => b.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .OrderByDescending(b => b.PreferredDate)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
