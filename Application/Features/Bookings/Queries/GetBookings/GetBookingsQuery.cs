using Application.Common.Models;
using Application.Interfaces.Repositories.Booking;
using Domain.Entities;
using MediatR;

namespace Application.Features.Bookings.Queries.GetBookings;

public record GetBookingsQuery : IRequest<Result<List<Domain.Entities.Booking>>>;

public class GetBookingsQueryHandler(IBookingReadRepository bookingReadRepository) : IRequestHandler<GetBookingsQuery, Result<List<Domain.Entities.Booking>>>
{
    public async Task<Result<List<Domain.Entities.Booking>>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await bookingReadRepository.GetAllAsync(cancellationToken);
        return Result<List<Domain.Entities.Booking>>.Success(bookings);
    }
}
