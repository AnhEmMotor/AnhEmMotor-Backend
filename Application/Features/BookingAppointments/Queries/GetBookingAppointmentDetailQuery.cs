using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Primitives;
using MediatR;

namespace Application.Features.BookingAppointments.Queries;

public class GetBookingAppointmentDetailQuery : IRequest<Result<Application.ApiContracts.BookingAppointments.Responses.BookingAppointmentResponse>>
{
	public int Id { get; set; }
}
