using Application.Interfaces.Repositories.ServiceBooking;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Repositories.ServiceBooking;

public class ServiceBookingUpdateRepository(ApplicationDBContext context) : IServiceBookingUpdateRepository
{
    public async Task<bool> UpdateStatusAsync(
        int id,
        string status,
        string reason,
        DateTime? cancelledAt,
        CancellationToken cancellationToken)
    {
        var booking = await context.ServiceBookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (booking == null)
            return false;
        booking.Status = status;
        booking.CancelledReason = reason ?? string.Empty;
        booking.CancelledDate = cancelledAt;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
