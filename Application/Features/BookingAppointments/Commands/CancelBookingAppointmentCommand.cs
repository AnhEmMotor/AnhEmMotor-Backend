using Application.Common.Models;
using MediatR;

namespace Application.Features.BookingAppointments.Commands;

public record CancelBookingAppointmentCommand(int Id, string? CancelReason) : IRequest<Result<bool>>;
