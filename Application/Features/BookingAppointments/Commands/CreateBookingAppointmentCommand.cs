using Application.Common.Models;
using MediatR;

namespace Application.Features.BookingAppointments.Commands;

public record CreateBookingAppointmentCommand(
    string FullName,
    string Phone,
    string? Email,
    string? ServiceType,
    DateTime? PreferredDate,
    string? PreferredTimeSlot,
    DateTimeOffset? AppointmentAt,
    string? Showroom,
    string? Notes) : IRequest<Result<int>>;
