using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Bookings.Commands.UpdateBooking
{
    public class UpdateBookingCommandHandler(
        IBookingReadRepository bookingReadRepository,
        IBookingInsertRepository bookingInsertRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateBookingCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await bookingReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (booking == null)
            {
                return Result<bool>.Failure(Error.NotFound("Lịch hẹn không tồn tại."));
            }

            booking.FullName = request.FullName;
            booking.PhoneNumber = request.PhoneNumber;
            booking.Email = request.Email ?? string.Empty;
            booking.PreferredDate = request.PreferredDate;
            booking.ProductVariantId = request.ProductVariantId;
            booking.Note = request.Note;
            booking.BookingType = request.BookingType;
            booking.Location = request.Location;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                booking.Status = request.Status;
            }

            bookingInsertRepository.Update(booking);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
    }
}
