using Application.Common.Models;
using MediatR;

namespace Application.Features.Bookings.Commands.DeleteBooking
{
    public class DeleteBookingCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }

        public DeleteBookingCommand(int id)
        {
            Id = id;
        }
    }
}
