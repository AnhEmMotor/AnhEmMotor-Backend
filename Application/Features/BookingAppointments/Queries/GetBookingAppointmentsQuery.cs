using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.BookingAppointments.Queries;

public class GetBookingAppointmentsQuery : IRequest<Result<PagedResult<Application.ApiContracts.BookingAppointments.Responses.BookingAppointmentResponse>>>
{
	public SieveModel Sieve { get; set; } = new();

	public DataFetchMode Mode { get; set; } = DataFetchMode.ActiveOnly;
}
