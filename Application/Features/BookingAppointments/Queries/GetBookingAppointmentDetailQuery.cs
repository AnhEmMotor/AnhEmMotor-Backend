using Application.ApiContracts.BookingAppointments.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.BookingAppointments.Queries;

public class GetBookingAppointmentDetailQuery : IRequest<Result<BookingAppointmentResponse>>
{
    public int Id { get; set; }
}
