using Application.Common.Models;
using MediatR;

namespace Application.Features.Bookings.Commands.ConfirmBooking;

public record ConfirmBookingCommand : IRequest<Result<bool>>
{
    public int BookingId { get; init; }
}

