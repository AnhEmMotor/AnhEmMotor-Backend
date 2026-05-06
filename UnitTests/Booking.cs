using Application.Common.Models;
using Application.Features.Bookings.Commands.ConfirmBooking;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Services;
using Domain.Constants.Booking;
using Domain.Constants.Lead;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests;

public class Booking
{
    private readonly Mock<IBookingReadRepository> _bookingReadRepoMock;
    private readonly Mock<IBookingInsertRepository> _bookingInsertRepoMock;
    private readonly Mock<ILeadReadRepository> _leadReadRepoMock;
    private readonly Mock<ILeadInsertRepository> _leadInsertRepoMock;
    private readonly Mock<ILeadActivityInsertRepository> _leadActivityInsertRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public Booking()
    {
        _bookingReadRepoMock = new Mock<IBookingReadRepository>();
        _bookingInsertRepoMock = new Mock<IBookingInsertRepository>();
        _leadReadRepoMock = new Mock<ILeadReadRepository>();
        _leadInsertRepoMock = new Mock<ILeadInsertRepository>();
        _leadActivityInsertRepoMock = new Mock<ILeadActivityInsertRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _emailServiceMock = new Mock<IEmailService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "BOOKING_002 - Tạo lịch hẹn tại địa điểm Showroom")]
    public async Task CreateBooking_LocationShowroom_SavesCorrectLocation()
    {
        // Arrange
        var command = new CreateBookingCommand { Location = BookingLocation.Showroom, PhoneNumber = "0909123456" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Lead { Id = 1 });

        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _bookingInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Booking>(b => b.Location == BookingLocation.Showroom)), Times.Once);
    }

    [Fact(DisplayName = "BOOKING_003 - Tạo lịch hẹn tại địa chỉ khách hàng")]
    public async Task CreateBooking_LocationCustomerAddress_SavesCorrectLocation()
    {
        // Arrange
        var command = new CreateBookingCommand { Location = BookingLocation.CustomerAddress, PhoneNumber = "0909123456" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Lead { Id = 1 });

        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _bookingInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Booking>(b => b.Location == BookingLocation.CustomerAddress)), Times.Once);
    }

    [Fact(DisplayName = "BOOKING_004 - Tự động gán loại hình Lái thử mặc định")]
    public void CreateBookingCommand_DefaultBookingType_IsTestDrive()
    {
        // Act
        var command = new CreateBookingCommand();

        // Assert
        command.BookingType.Should().Be(BookingType.TestDrive);
    }

    [Fact(DisplayName = "BOOKING_006 - Kiểm tra logic lọc BookingType khi tạo Activity")]
    public async Task CreateBooking_TestDriveType_ActivityDescriptionContainsVietnamese()
    {
        // Arrange
        var command = new CreateBookingCommand { BookingType = BookingType.TestDrive, PhoneNumber = "0909123456" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Lead { Id = 1 });

        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _leadActivityInsertRepoMock.Verify(x => x.Add(It.Is<LeadActivity>(a => a.Description.Contains("Lái thử"))), Times.Once);
    }

    [Fact(DisplayName = "BOOKING_009 - Xử lý khi xác nhận lịch hẹn không tồn tại")]
    public async Task ConfirmBooking_IdNotExists_ReturnsFailure()
    {
        // Arrange
        var command = new ConfirmBookingCommand { BookingId = 999 };
        _bookingReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Booking?)null);

        var handler = new ConfirmBookingCommandHandler(
            _bookingReadRepoMock.Object,
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Lịch hẹn không tồn tại.");
    }

    [Fact(DisplayName = "BOOKING_011 - Kiểm tra nội dung email chứa thông tin biến thể xe")]
    public async Task ConfirmBooking_ValidBooking_EmailContainsVariantInfo()
    {
        // Arrange
        var booking = new Domain.Entities.Booking 
        { 
            Id = 1, 
            PhoneNumber = "0909123456",
            Email = "test@gmail.com",
            FullName = "Test User",
            ProductVariant = new ProductVariant 
            { 
                VersionName = "Standard",
                Product = new Domain.Entities.Product { Name = "Honda SH" }
            }
        };
        _bookingReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Lead { Id = 1 });

        var handler = new ConfirmBookingCommandHandler(
            _bookingReadRepoMock.Object,
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);

        // Act
        await handler.Handle(new ConfirmBookingCommand { BookingId = 1 }, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.Is<string>(body => body.Contains("Honda SH") && body.Contains("Standard")), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "BOOKING_013 - Xử lý lỗi khi gửi email không thành công")]
    public async Task ConfirmBooking_EmailServiceThrows_StillSucceedsInDB()
    {
        // Arrange
        var booking = new Domain.Entities.Booking { Id = 1, PhoneNumber = "0909123456", Email = "test@gmail.com" };
        _bookingReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email service down"));

        var handler = new ConfirmBookingCommandHandler(
            _bookingReadRepoMock.Object,
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);

        // Act
        var result = await handler.Handle(new ConfirmBookingCommand { BookingId = 1 }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "BOOKING_014 - Kiểm tra định dạng ngày tháng trong email")]
    public async Task ConfirmBooking_ValidBooking_EmailHasCorrectDateFormat()
    {
        // Arrange
        var preferredDate = new DateTimeOffset(2026, 5, 20, 10, 30, 0, TimeSpan.Zero);
        var booking = new Domain.Entities.Booking 
        { 
            Id = 1, 
            PhoneNumber = "0909123456", 
            PreferredDate = preferredDate 
        };
        _bookingReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var handler = new ConfirmBookingCommandHandler(
            _bookingReadRepoMock.Object,
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);

        // Act
        await handler.Handle(new ConfirmBookingCommand { BookingId = 1 }, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.Is<string>(body => body.Contains("20/05/2026 10:30")), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "BOOKING_015 - Đảm bảo tính nhất quán của ID biến thể xe")]
    public async Task CreateBooking_ValidVariantId_SavesCorrectVariantId()
    {
        // Arrange
        var command = new CreateBookingCommand { ProductVariantId = 101, PhoneNumber = "0909123456" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Lead { Id = 1 });

        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _bookingInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Booking>(b => b.ProductVariantId == 101)), Times.Once);
    }

    [Fact(DisplayName = "BOOKING_017 - Kiểm tra logic gán LeadSource khi tạo Booking")]
    public async Task CreateBooking_NewLead_SetsSourceToWebStore()
    {
        // Arrange
        var command = new CreateBookingCommand { PhoneNumber = "0909888777", FullName = "New Lead" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead?)null);

        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<Lead>(l => l.Source == LeadSource.WebStore)), Times.Once);
    }
}
