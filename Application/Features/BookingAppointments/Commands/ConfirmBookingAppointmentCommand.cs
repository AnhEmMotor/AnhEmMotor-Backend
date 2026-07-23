using Application.Common.Models;
using MediatR;

namespace Application.Features.BookingAppointments.Commands;

public record ConfirmBookingAppointmentCommand(int Id, DateTimeOffset? AppointmentAt, Guid CurrentUserId) : IRequest<Result<bool>>;
