using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Domain.Constants.Booking;
using MediatR;

namespace Application.Features.Bookings.Commands.UpdateBooking
{
    public class UpdateBookingCommandHandler(
        IBookingReadRepository bookingReadRepository,
        IBookingInsertRepository bookingInsertRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateBookingCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await bookingReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (booking == null)
            {
                return Result<bool>.Failure(Error.NotFound("Lịch hẹn không tồn tại."));
            }

            // Check for overlap: same PreferredDate, status is not Cancelled, and not this booking itself
            var allBookings = await bookingReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var isOverlap = allBookings.Any(b => b.PreferredDate == request.PreferredDate && b.Status != BookingStatus.Cancelled && b.Id != request.Id);
            if (isOverlap)
            {
                return Result<bool>.Failure("Thời gian đặt lịch này đã bị trùng với lịch hẹn khác.");
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
