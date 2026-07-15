using Application.Common.Models;
using MediatR;

namespace Application.Features.BookingAppointments.Commands;

public record DeleteBookingAppointmentCommand(int Id) : IRequest<Result<bool>>;
