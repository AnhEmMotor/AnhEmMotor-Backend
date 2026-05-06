using Application.Common.Models;
using Application.Interfaces.Repositories.Booking;
using Domain.Entities;
using MediatR;

namespace Application.Features.Bookings.Queries.GetBookings;

public record GetBookingsQuery : IRequest<Result<List<Booking>>>;

public class GetBookingsQueryHandler(IBookingReadRepository bookingReadRepository) : IRequestHandler<GetBookingsQuery, Result<List<Booking>>>
{
    public async Task<Result<List<Booking>>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await bookingReadRepository.GetAllAsync(cancellationToken);
        return Result<List<Booking>>.Success(bookings);
    }
}
