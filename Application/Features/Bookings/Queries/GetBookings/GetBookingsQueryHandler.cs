using Application.Common.Models;
using Application.Interfaces.Repositories.Booking;
using Domain.Entities;
using MediatR;

namespace Application.Features.Bookings.Queries.GetBookings;

public class GetBookingsQueryHandler(IBookingReadRepository bookingReadRepository) : IRequestHandler<GetBookingsQuery, Result<List<Booking>>>
{
    public async Task<Result<List<Booking>>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await bookingReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Result<List<Booking>>.Success(bookings);
    }
}
