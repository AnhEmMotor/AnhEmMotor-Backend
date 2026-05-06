using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Bookings.Commands.CreateBooking;

public record CreateBookingCommand : IRequest<Result<int>>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public int ProductVariantId { get; init; }
    public DateTimeOffset PreferredDate { get; init; }
    public string Note { get; init; } = string.Empty;
    public string Location { get; init; } = "Showroom";
    public string BookingType { get; init; } = "TestDrive";
}

public class CreateBookingCommandHandler(
    IBookingInsertRepository bookingInsertRepository,
    ILeadReadRepository leadReadRepository,
    ILeadWriteRepository leadWriteRepository,
    ILeadActivityInsertRepository leadActivityInsertRepository,
    ILeadAssignmentService leadAssignmentService,
    INotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBookingCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Giai đoạn Phân phối & Thông báo (Lead logic)
        var lead = await leadReadRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        
        if (lead == null)
        {
            // Nếu SĐT mới: Khởi tạo một Hồ sơ khách hàng tiềm năng (Lead) mới
            lead = new Lead
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Score = 30, // Cộng +30 điểm cho Lead này vì có nhu cầu thực tế
                Status = "New",
                Source = "WebStore"
            };

            // Tự động gán cho Sale (Round Robin)
            await leadAssignmentService.AssignLeadAsync(lead, cancellationToken);

            leadWriteRepository.Add(lead);
            
            // Add activity
            leadActivityInsertRepository.Add(new LeadActivity
            {
                Lead = lead,
                ActivityType = "Booking",
                Description = $"Đăng ký {(request.BookingType == "TestDrive" ? "Lái thử" : request.BookingType)} mới tại {request.Location}. (Khách hàng mới)",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            // Nếu SĐT đã tồn tại: Cập nhật yêu cầu lái thử này vào Dòng thời gian
            lead.Score += 30; // Tăng điểm vì quay lại đăng ký
            if (lead.Score > 100) lead.Score = 100;

            leadWriteRepository.Update(lead);
            
            leadActivityInsertRepository.Add(new LeadActivity
            {
                LeadId = lead.Id,
                ActivityType = "Booking",
                Description = $"Đăng ký {(request.BookingType == "TestDrive" ? "Lái thử" : request.BookingType)} mới tại {request.Location}.",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        // 2. Tạo Booking
        var booking = new Domain.Entities.Booking
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ProductVariantId = request.ProductVariantId,
            PreferredDate = request.PreferredDate,
            Note = request.Note,
            Status = "Pending",
            Location = request.Location,
            BookingType = request.BookingType
        };

        bookingInsertRepository.Add(booking);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Giai đoạn 2 - Real-time Alert (SSE)
        notificationService.NotifyNewBooking($"Có yêu cầu lái thử mới từ khách hàng {request.FullName} ({request.PhoneNumber})");
        
        return Result<int>.Success(booking.Id);
    }
}
