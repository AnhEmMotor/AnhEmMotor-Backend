using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using System;
using Domain.Constants.Booking;
using MediatR;

namespace Application.Features.Bookings.Commands.CreateBooking;

public record CreateBookingCommand : IRequest<Result<int>>
{
    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public int? ProductVariantId { get; init; }

    public DateTimeOffset PreferredDate { get; init; }

    public string Note { get; init; } = string.Empty;

    public string Location { get; init; } = "Showroom";

    public string BookingType { get; init; } = "TestDrive";
}
    public string Location { get; init; } = BookingLocation.Showroom;

    public string BookingType { get; init; } = Domain.Constants.Booking.BookingType.TestDrive;
}
