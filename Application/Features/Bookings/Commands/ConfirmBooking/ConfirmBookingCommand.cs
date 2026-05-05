using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Lead;
using Domain.Entities;
using MediatR;

namespace Application.Features.Bookings.Commands.ConfirmBooking;

public record ConfirmBookingCommand : IRequest<Result<bool>>
{
    public int BookingId { get; init; }
}

