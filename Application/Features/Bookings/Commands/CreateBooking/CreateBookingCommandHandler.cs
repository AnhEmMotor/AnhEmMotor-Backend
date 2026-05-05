using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Services;
using Domain.Constants.Booking;
using Domain.Constants.Lead;
using Domain.Entities;
using MediatR;

namespace Application.Features.Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandler(
    IBookingInsertRepository bookingInsertRepository,
    ILeadReadRepository leadReadRepository,
    ILeadInsertRepository leadInsertRepository,
    ILeadActivityInsertRepository leadActivityInsertRepository,
    INotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBookingCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var lead = await leadReadRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken)
            .ConfigureAwait(false);
        if (lead == null)
        {
            lead = new Lead
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Score = 30,
                Status = LeadStatus.Consulting,
                Source = LeadSource.WebStore
            };
            leadInsertRepository.Add(lead);
            leadActivityInsertRepository.Add(
                new LeadActivity
                {
                    Lead = lead,
                    ActivityType = LeadActivityType.Booking,
                    Description =
                        $"Đăng ký {(string.Compare(request.BookingType, BookingType.TestDrive, StringComparison.Ordinal) == 0 ? "Lái thử" : request.BookingType)} mới tại {request.Location}. (Khách hàng mới)",
                    CreatedAt = DateTimeOffset.UtcNow
                });
        } else
        {
            lead.Score += 30;
            leadInsertRepository.Update(lead);
            leadActivityInsertRepository.Add(
                new LeadActivity
                {
                    LeadId = lead.Id,
                    ActivityType = LeadActivityType.Booking,
                    Description =
                        $"Đăng ký {(string.Compare(request.BookingType, BookingType.TestDrive, StringComparison.Ordinal) == 0 ? "Lái thử" : request.BookingType)} mới tại {request.Location}.",
                    CreatedAt = DateTimeOffset.UtcNow
                });
        }
        var booking = new Booking
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ProductVariantId = request.ProductVariantId,
            PreferredDate = request.PreferredDate,
            Note = request.Note,
            Status = BookingStatus.Pending,
            Location = request.Location,
            BookingType = request.BookingType
        };
        bookingInsertRepository.Add(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        notificationService.NotifyNewBooking(
            $"Có yêu cầu lái thử mới từ khách hàng {request.FullName} ({request.PhoneNumber})");
        return Result<int>.Success(booking.Id);
    }
}
