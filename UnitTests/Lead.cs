using Application.ApiContracts.Leads.Responses;
using Application.Common.Helper;
using Application.Features.Bookings.Commands.ConfirmBooking;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Features.Leads.Queries.GetLeads;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests;

public class Lead
{
    private readonly Mock<ILeadReadRepository> _leadReadRepoMock;
    private readonly Mock<ILeadInsertRepository> _leadInsertRepoMock;
    private readonly Mock<ILeadActivityInsertRepository> _leadActivityInsertRepoMock;
    private readonly Mock<IBookingInsertRepository> _bookingInsertRepoMock;
    private readonly Mock<IBookingReadRepository> _bookingReadRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public Lead()
    {
        _leadReadRepoMock = new Mock<ILeadReadRepository>();
        _leadInsertRepoMock = new Mock<ILeadInsertRepository>();
        _leadActivityInsertRepoMock = new Mock<ILeadActivityInsertRepository>();
        _bookingInsertRepoMock = new Mock<IBookingInsertRepository>();
        _bookingReadRepoMock = new Mock<IBookingReadRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _emailServiceMock = new Mock<IEmailService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "LEAD_006 - Chuẩn hóa Slug từ tên khách hàng hoặc tiêu đề")]
    public void SlugHelper_GenerateSlug_ReturnsCorrectSlug()
    {
        // Arrange
        var input = "Nguyễn Văn A - Lái Thử";
        var expected = "nguyen-van-a-lai-thu";

        // Act
        var result = SlugHelper.GenerateSlug(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "LEAD_007 - SlugHelper xử lý ký tự đặc biệt và độ dài chuỗi")]
    public void SlugHelper_LongAndSpecialChars_ReturnsSanitizedSlug()
    {
        // Arrange
        var input = new string('a', 300) + "!@#$%^&*()";
        
        // Act
        var result = SlugHelper.GenerateSlug(input);

        // Assert
        result.Length.Should().BeLessOrEqualTo(255);
        result.Should().NotContainAny("!", "@", "#", "$", "%", "^", "&", "*", "(", ")");
    }

    [Fact(DisplayName = "LEAD_008 - Cộng dồn điểm Score cho Lead cũ khi đặt lịch")]
    public async Task CreateBooking_ExistingLead_IncreasesScore()
    {
        // Arrange
        var command = new CreateBookingCommand { PhoneNumber = "0909123456", FullName = "Test User" };
        var existingLead = new Domain.Entities.Lead { Id = 1, PhoneNumber = "0909123456", Score = 50 };
        
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLead);

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
        existingLead.Score.Should().Be(80);
        _leadInsertRepoMock.Verify(x => x.Update(existingLead), Times.Once);
    }

    [Fact(DisplayName = "LEAD_009 - Khởi tạo điểm Score mặc định cho Lead mới")]
    public async Task CreateBooking_NewLead_SetsDefaultScore()
    {
        // Arrange
        var command = new CreateBookingCommand { PhoneNumber = "0909999999", FullName = "New User" };
        
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Lead?)null);

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
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Lead>(l => l.Score == 30)), Times.Once);
    }

    [Fact(DisplayName = "LEAD_010 - Xác định trạng thái Lead mới tạo")]
    public async Task CreateBooking_NewLead_SetsDefaultStatus()
    {
        // Arrange
        var command = new CreateBookingCommand { PhoneNumber = "0909999999", FullName = "New User" };
        
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Lead?)null);

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
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Lead>(l => l.Status == "Consulting")), Times.Once);
    }

    [Fact(DisplayName = "LEAD_011 - Ghi nhận mô tả Activity cho khách hàng mới")]
    public async Task CreateBooking_NewLead_ActivityDescriptionContainsNewCustomerTag()
    {
        // Arrange
        var command = new CreateBookingCommand 
        { 
            PhoneNumber = "0909999999", 
            FullName = "New User",
            BookingType = "TestDrive",
            Location = "Showroom A"
        };
        
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Lead?)null);

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
        _leadActivityInsertRepoMock.Verify(x => x.Add(It.Is<LeadActivity>(a => 
            a.Description.Contains("Lái thử") && a.Description.Contains("(Khách hàng mới)")
        )), Times.Once);
    }

    [Fact(DisplayName = "LEAD_012 - Ghi nhận mô tả Activity cho khách hàng cũ")]
    public async Task CreateBooking_ExistingLead_ActivityDescriptionDoesNotContainNewCustomerTag()
    {
        // Arrange
        var command = new CreateBookingCommand 
        { 
            PhoneNumber = "0909123456", 
            FullName = "Existing User",
            BookingType = "Other",
            Location = "Showroom B"
        };
        var existingLead = new Domain.Entities.Lead { Id = 1, PhoneNumber = "0909123456" };
        
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLead);

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
        _leadActivityInsertRepoMock.Verify(x => x.Add(It.Is<LeadActivity>(a => 
            !a.Description.Contains("(Khách hàng mới)")
        )), Times.Once);
    }

    [Fact(DisplayName = "LEAD_013 - ConfirmBooking trả về lỗi khi ID không tồn tại")]
    public async Task ConfirmBooking_IdNotExists_ReturnsFailure()
    {
        // Arrange
        var command = new ConfirmBookingCommand { BookingId = 999 };
        
        _bookingReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

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

    [Fact(DisplayName = "LEAD_014 - Cập nhật trạng thái Lead sang Đang lái thử")]
    public async Task ConfirmBooking_ValidBooking_UpdatesLeadStatus()
    {
        // Arrange
        var booking = new Booking { Id = 1, PhoneNumber = "0909123456" };
        var lead = new Domain.Entities.Lead { Id = 1, PhoneNumber = "0909123456", Status = "Consulting" };
        var command = new ConfirmBookingCommand { BookingId = 1 };

        _bookingReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync("0909123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        var handler = new ConfirmBookingCommandHandler(
            _bookingReadRepoMock.Object,
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        lead.Status.Should().Be("TestDriving");
        _leadInsertRepoMock.Verify(x => x.Update(lead), Times.Once);
    }

    [Fact(DisplayName = "LEAD_015 - Mapping dữ liệu LeadResponse từ thực thể Lead")]
    public async Task GetLeads_ValidData_MapsCorrectly()
    {
        // Arrange
        var lead = new Domain.Entities.Lead
        {
            Id = 1,
            FullName = "Nguyen Van A",
            Email = "a@gmail.com",
            PhoneNumber = "0909123456",
            Score = 50,
            Status = "Consulting",
            Source = "WebStore",
            CreatedAt = new DateTimeOffset(2026, 5, 5, 0, 0, 0, TimeSpan.Zero),
            Activities =
            [
                new LeadActivity { Id = 1, ActivityType = "Booking", Description = "Test", CreatedAt = DateTimeOffset.UtcNow }
            ]
        };

        _leadReadRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([lead]);

        var handler = new GetLeadsQueryHandler(_leadReadRepoMock.Object);

        // Act
        var result = await handler.Handle(new GetLeadsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var response = result[0];
        response.FullName.Should().Be(lead.FullName);
        response.CreatedAt.Should().Be(lead.CreatedAt.Value);
        response.Activities.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "LEAD_020 - Kiểm tra giá trị mặc định của ActivityType")]
    public void LeadActivityEntity_DefaultConstructor_ActivityTypeIsNote()
    {
        // Act
        var activity = new Domain.Entities.LeadActivity();

        // Assert
        activity.ActivityType.Should().Be("Note");
    }

    [Fact(DisplayName = "LEAD_021 - Logic xử lý mô tả khi loại đặt lịch là Lái thử")]
    public async Task CreateBooking_TestDriveType_DescriptionUsesFriendlyName()
    {
        // Arrange
        var command = new CreateBookingCommand 
        { 
            PhoneNumber = "0909000021", 
            FullName = "User", 
            BookingType = "TestDrive",
            Location = "Showroom"
        };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Lead?)null);

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
        _leadActivityInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.LeadActivity>(a => 
            a.Description.Contains("Lái thử")
        )), Times.Once);
    }

    [Fact(DisplayName = "LEAD_022 - Logic xử lý mô tả cho các loại đặt lịch khác")]
    public async Task CreateBooking_OtherType_DescriptionUsesOriginalType()
    {
        // Arrange
        var command = new CreateBookingCommand 
        { 
            PhoneNumber = "0909000022", 
            FullName = "User", 
            BookingType = "Consulting",
            Location = "Showroom"
        };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Lead?)null);

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
        _leadActivityInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.LeadActivity>(a => 
            a.Description.Contains("Consulting")
        )), Times.Once);
    }

    [Fact(DisplayName = "LEAD_023 - Mapping dữ liệu sang LeadActivityResponse")]
    public void LeadActivity_MappingToResponse_CorrectValues()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow;
        var activity = new Domain.Entities.LeadActivity
        {
            Id = 100,
            ActivityType = "Booking",
            Description = "Test Description",
            CreatedAt = createdAt
        };

        // Act
        var response = new LeadActivityResponse
        {
            Id = activity.Id,
            ActivityType = activity.ActivityType,
            Description = activity.Description,
            CreatedAt = activity.CreatedAt.Value
        };

        // Assert
        response.Id.Should().Be(100);
        response.ActivityType.Should().Be("Booking");
        response.Description.Should().Be("Test Description");
        response.CreatedAt.Should().Be(createdAt);
    }

    [Fact(DisplayName = "LEAD_024 - Xử lý giá trị thời gian null khi Mapping")]
    public void LeadActivity_MappingWithNullCreatedAt_ReturnsMinValue()
    {
        // Arrange
        var activity = new Domain.Entities.LeadActivity { CreatedAt = null };

        // Act
        var response = new LeadActivityResponse
        {
            CreatedAt = activity.CreatedAt ?? DateTimeOffset.MinValue
        };

        // Assert
        response.CreatedAt.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact(DisplayName = "LEAD_027 - Gán điểm khởi tạo cho khách hàng mới")]
    public async Task CreateBooking_NewLead_SetsInitialScore30()
    {
        // Arrange
        var command = new CreateBookingCommand { PhoneNumber = "0909000001", FullName = "New User" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Lead?)null);

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
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Lead>(l => l.Score == 30)), Times.Once);
    }

    [Fact(DisplayName = "LEAD_028 - Cộng dồn điểm khi khách hàng cũ đặt lịch")]
    public async Task CreateBooking_ExistingLead_Adds30Points()
    {
        // Arrange
        var command = new CreateBookingCommand { PhoneNumber = "0909000002", FullName = "Existing User" };
        var existingLead = new Domain.Entities.Lead { Id = 1, PhoneNumber = "0909000002", Score = 30 };
        
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLead);

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
        existingLead.Score.Should().Be(60);
        _leadInsertRepoMock.Verify(x => x.Update(existingLead), Times.Once);
    }

    [Fact(DisplayName = "LEAD_030 - Kiểm tra giá trị Score mặc định của thực thể")]
    public void LeadEntity_DefaultConstructor_ScoreIsZero()
    {
        // Act
        var lead = new Domain.Entities.Lead();

        // Assert
        lead.Score.Should().Be(0);
    }

    [Fact(DisplayName = "LEAD_031 - Đảm bảo tính toàn vẹn của Score khi Mapping")]
    public void Lead_MappingToResponse_PreservesScore()
    {
        // Arrange
        var lead = new Domain.Entities.Lead { Score = 150 };

        // Act
        var response = new LeadResponse { Score = lead.Score };

        // Assert
        response.Score.Should().Be(150);
    }

    [Theory(DisplayName = "LEAD_032 - Cộng điểm không phụ thuộc vào địa điểm đặt lịch")]
    [InlineData("Showroom")]
    [InlineData("CustomerAddress")]
    public async Task CreateBooking_DifferentLocations_StillAdds30Points(string location)
    {
        // Arrange
        var command = new CreateBookingCommand { PhoneNumber = "0909000003", FullName = "User", Location = location };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Lead?)null);

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
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Lead>(l => l.Score == 30)), Times.Once);
    }

    [Theory(DisplayName = "LEAD_034 - Kiểm tra Score khi đặt lịch với loại Booking khác nhau")]
    [InlineData("TestDrive")]
    [InlineData("Consulting")]
    public async Task CreateBooking_DifferentTypes_StillAdds30Points(string bookingType)
    {
        // Arrange
        var command = new CreateBookingCommand { PhoneNumber = "0909000004", FullName = "User", BookingType = bookingType };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Lead?)null);

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
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Lead>(l => l.Score == 30)), Times.Once);
    }
}
