using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Bookings.Commands.ConfirmBooking;

public record ConfirmBookingCommand : IRequest<Result<bool>>
{
    public int BookingId { get; init; }
}

public class ConfirmBookingCommandHandler(
    IBookingReadRepository bookingReadRepository,
    IBookingInsertRepository bookingInsertRepository,
    ILeadReadRepository leadReadRepository,
    ILeadWriteRepository leadWriteRepository,
    ILeadActivityInsertRepository leadActivityInsertRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService) : IRequestHandler<ConfirmBookingCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingReadRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result<bool>.Failure(Error.NotFound("Lịch hẹn không tồn tại."));
        }
        booking.Status = "Confirmed";
        bookingInsertRepository.Update(booking);
        var lead = await leadReadRepository.GetByPhoneNumberAsync(booking.PhoneNumber, cancellationToken);
        if (lead != null)
        {
            lead.Status = "TestDriving";
            leadWriteRepository.Update(lead);
            leadActivityInsertRepository.Add(
                new LeadActivity
                {
                    LeadId = lead.Id,
                    ActivityType = "Contact",
                    Description =
                        $"Xác nhận lịch hẹn lái thử cho {booking.ProductVariant?.Product?.Name}. Chuyển trạng thái sang Đang lái thử.",
                    CreatedAt = DateTimeOffset.UtcNow
                });
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var googleMapsLink = "https://www.google.com/maps/search/?api=1&query=Showroom+AnhEm+Motor+Biên+Hòa";
        var subject = "Xác nhận lịch hẹn lái thử | AnhEm Motor";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                <h2 style='color: #e31837; text-transform: uppercase;'>Xác nhận lịch hẹn thành công</h2>
                <p>Chào <strong>{booking.FullName}</strong>,</p>
                <p>Lịch hẹn lái thử xe của bạn đã được quản trị viên xác nhận. Chúng tôi rất mong được đón tiếp bạn!</p>
                
                <div style='background: #f9f9f9; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 5px 0;'><strong>Thời gian:</strong> {booking.PreferredDate:dd/MM/yyyy HH:mm}</p>
                    <p style='margin: 5px 0;'><strong>Dòng xe:</strong> {booking.ProductVariant?.Product?.Name} - {booking.ProductVariant?.VersionName}</p>
                    <p style='margin: 5px 0;'><strong>Địa điểm:</strong> {booking.Location}</p>
                </div>

                <p>📍 <strong>Chỉ đường Google Maps:</strong> <a href='{googleMapsLink}' style='color: #e31837; text-decoration: none; font-weight: bold;'>Bấm vào đây để xem đường đi</a></p>
                
                <p style='color: #666; font-size: 13px; font-style: italic; margin-top: 20px;'>
                    * Lưu ý: Khi đến vui lòng mang theo bằng lái xe máy còn hạn sử dụng.
                </p>
                
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                <p style='font-size: 12px; color: #999; text-align: center;'>
                    AnhEm Motor - Hệ thống bán lẻ xe máy uy tín hàng đầu<br/>
                    Hotline: 0909 xxx xxx | Website: anhemmotor.com
                </p>
            </div>";
        try
        {
            await emailService.SendEmailAsync(booking.Email, subject, body);
        } catch
        {
        }
        return Result<bool>.Success(true);
    }
}
