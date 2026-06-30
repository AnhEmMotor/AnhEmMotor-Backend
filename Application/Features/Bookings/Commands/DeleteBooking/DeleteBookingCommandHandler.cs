using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Bookings.Commands.DeleteBooking
{
    public class DeleteBookingCommandHandler(
        IBookingReadRepository bookingReadRepository,
        IBookingInsertRepository bookingInsertRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteBookingCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await bookingReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (booking == null)
            {
                return Result<bool>.Failure(Error.NotFound("Lịch hẹn không tồn tại."));
            }

            bookingInsertRepository.Remove(booking);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
    }
}
