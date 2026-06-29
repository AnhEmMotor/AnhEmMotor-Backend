using Application.ApiContracts.PlateDossier.Responses;
using Application.Common.Helper;
using Application.Features.Bookings.Commands.ConfirmBooking;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Features.Customer.Queries.GetCustomerProfile360;
using Application.Features.Leads.Commands.AddLeadActivity;
using Application.Features.Leads.Queries.GetLeads;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Booking;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Lead.LeadActivity;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.PlateDossier;
using Application.Interfaces.Repositories.RepairOrder;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Moq;
using Sieve.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using BookingEntity = Domain.Entities.Booking;
using LeadEntity = Domain.Entities.Lead;

namespace UnitTests;

public class Lead
{
    private readonly Mock<ILeadReadRepository> _leadReadRepoMock;
    private readonly Mock<ILeadInsertRepository> _leadInsertRepoMock;
    private readonly Mock<ILeadActivityInsertRepository> _leadActivityInsertRepoMock;
    private readonly Mock<IBookingInsertRepository> _bookingInsertRepoMock;
    private readonly Mock<IBookingReadRepository> _bookingReadRepoMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILeadUpdateRepository> _leadUpdateRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOutputReadRepository> _outputReadRepoMock;
    private readonly Mock<IPlateDossierReadRepository> _plateDossierReadRepoMock;
    private readonly Mock<IRepairOrderReadRepository> _repairOrderReadRepoMock;
    private readonly Mock<IVehicleReadRepository> _vehicleReadRepoMock;

    public Lead()
    {
        _leadReadRepoMock = new Mock<ILeadReadRepository>();
        _leadInsertRepoMock = new Mock<ILeadInsertRepository>();
        _leadActivityInsertRepoMock = new Mock<ILeadActivityInsertRepository>();
        _bookingInsertRepoMock = new Mock<IBookingInsertRepository>();
        _bookingReadRepoMock = new Mock<IBookingReadRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _leadUpdateRepoMock = new Mock<ILeadUpdateRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _outputReadRepoMock = new Mock<IOutputReadRepository>();
        _plateDossierReadRepoMock = new Mock<IPlateDossierReadRepository>();
        _repairOrderReadRepoMock = new Mock<IRepairOrderReadRepository>();
        _vehicleReadRepoMock = new Mock<IVehicleReadRepository>();
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(DisplayName = "LEAD_006 - Chuẩn hóa Slug từ tên khách hàng hoặc tiêu đề")]
    public void SlugHelper_GenerateSlug_ReturnsCorrectSlug()
    {
        var InventoryReceipt = "Nguyễn Văn A - Lái Thử";
        var expected = "nguyen-van-a-lai-thu";
        var result = SlugHelper.GenerateSlug(InventoryReceipt);
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "LEAD_007 - SlugHelper xử lý ký tự đặc biệt và độ dài chuỗi")]
    public void SlugHelper_LongAndSpecialChars_ReturnsSanitizedSlug()
    {
        var InventoryReceipt = $"{new string('a', 300)}!@#$%^&*()";
        var result = SlugHelper.GenerateSlug(InventoryReceipt);
        result.Length.Should().BeLessThanOrEqualTo(255);
        result.Should().NotContainAny("!", "@", "#", "$", "%", "^", "&", "*", "(", ")");
    }

    [Fact(DisplayName = "LEAD_008 - Cộng dồn điểm Score cho Lead cũ khi đặt lịch")]
    public async Task CreateBooking_ExistingLead_IncreasesScore()
    {
        var command = new CreateBookingCommand { PhoneNumber = "0909123456", FullName = "Test User" };
        var existingLead = new LeadEntity { Id = 1, PhoneNumber = "0909123456", Score = 50 };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLead);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        existingLead.Score.Should().Be(80);
        _leadUpdateRepoMock.Verify(x => x.Update(existingLead), Times.Once);
    }

    [Fact(DisplayName = "LEAD_009 - Khởi tạo điểm Score mặc định cho Lead mới")]
    public async Task CreateBooking_NewLead_SetsDefaultScore()
    {
        var command = new CreateBookingCommand { PhoneNumber = "0909999999", FullName = "New User" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadEntity?)null);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<LeadEntity>(l => l.Score == 30)), Times.Once);
    }

    [Fact(DisplayName = "LEAD_010 - Xác định trạng thái Lead mới tạo")]
    public async Task CreateBooking_NewLead_SetsDefaultStatus()
    {
        var command = new CreateBookingCommand { PhoneNumber = "0909999999", FullName = "New User" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadEntity?)null);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _leadInsertRepoMock.Verify(
            x => x.Add(It.Is<LeadEntity>(l => string.Compare(l.Status, "Consulting") == 0)),
            Times.Once);
    }

    [Fact(DisplayName = "LEAD_011 - Ghi nhận mô tả Activity cho khách hàng mới")]
    public async Task CreateBooking_NewLead_ActivityDescriptionContainsNewCustomerTag()
    {
        var command = new CreateBookingCommand
        {
            PhoneNumber = "0909999999",
            FullName = "New User",
            BookingType = "TestDrive",
            Location = "Showroom A"
        };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadEntity?)null);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _leadActivityInsertRepoMock.Verify(
            x => x.Add(
                It.Is<LeadActivity>(
                    a => a.Description.Contains("Lái thử") && a.Description.Contains("(Khách hàng mới)"))),
            Times.Once);
    }

    [Fact(DisplayName = "LEAD_012 - Ghi nhận mô tả Activity cho khách hàng cũ")]
    public async Task CreateBooking_ExistingLead_ActivityDescriptionDoesNotContainNewCustomerTag()
    {
        var command = new CreateBookingCommand
        {
            PhoneNumber = "0909123456",
            FullName = "Existing User",
            BookingType = "Other",
            Location = "Showroom B"
        };
        var existingLead = new LeadEntity { Id = 1, PhoneNumber = "0909123456" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLead);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _leadActivityInsertRepoMock.Verify(
            x => x.Add(It.Is<LeadActivity>(a => !a.Description.Contains("(Khách hàng mới)"))),
            Times.Once);
    }

    [Fact(DisplayName = "LEAD_013 - ConfirmBooking trả về lỗi khi ID không tồn tại")]
    public async Task ConfirmBooking_IdNotExists_ReturnsFailure()
    {
        var command = new ConfirmBookingCommand { BookingId = 999 };
        _bookingReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookingEntity?)null);
        var handler = new ConfirmBookingCommandHandler(
            _bookingReadRepoMock.Object,
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => string.Compare(e.Message, "Lịch hẹn không tồn tại.") == 0);
    }

    [Fact(DisplayName = "LEAD_014 - Cập nhật trạng thái Lead sang Đang lái thử")]
    public async Task ConfirmBooking_ValidBooking_UpdatesLeadStatus()
    {
        var booking = new BookingEntity { Id = 1, PhoneNumber = "0909123456" };
        var lead = new LeadEntity { Id = 1, PhoneNumber = "0909123456", Status = "Consulting" };
        var command = new ConfirmBookingCommand { BookingId = 1 };
        _bookingReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync("0909123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        var handler = new ConfirmBookingCommandHandler(
            _bookingReadRepoMock.Object,
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        lead.Status.Should().Be("TestDriving");
        _leadUpdateRepoMock.Verify(x => x.Update(lead), Times.Once);
    }

    [Fact(DisplayName = "LEAD_015 - Mapping dữ liệu LeadResponse từ thực thể Lead")]
    public async Task GetLeads_ValidData_MapsCorrectly()
    {
        var lead = new LeadEntity
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
                [new LeadActivity
                {
                    Id = 1,
                    ActivityType = "Booking",
                    Description = "Test",
                    CreatedAt = DateTimeOffset.UtcNow
                }]
        };
        _leadReadRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([lead]);
        var handler = new GetLeadsQueryHandler(_leadReadRepoMock.Object);
        var result = await handler.Handle(new GetLeadsQuery(), CancellationToken.None).ConfigureAwait(true);
        result.Should().HaveCount(1);
        var response = result[0];
        response.FullName.Should().Be(lead.FullName);
        response.CreatedAt.Should().Be(lead.CreatedAt.Value);
        response.Activities.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "LEAD_016 - Mapping day du thong tin ho so khach hang khi lay danh sach Lead")]
    public async Task GetLeads_ShouldMapCustomerProfileFields()
    {
        var assignedToId = Guid.NewGuid();
        var lead = new LeadEntity
        {
            Id = 1,
            FullName = "Nguyen Van A",
            Email = "a@gmail.com",
            PhoneNumber = "0909123456",
            Score = 50,
            Status = "Consulting",
            Source = "WebStore",
            InterestedVehicle = "Honda SH 160i",
            AddressDetail = "12 Nguyen Trai",
            Ward = "Ward 1",
            District = "District 5",
            Province = "Ho Chi Minh",
            Gender = "Male",
            Birthday = new DateTime(1995, 6, 20),
            IdentificationNumber = "079095000001",
            CreatedAt = new DateTimeOffset(2026, 5, 5, 0, 0, 0, TimeSpan.Zero),
            IsVerified = true,
            Tier = "Gold",
            Points = 120,
            AssignedToId = assignedToId,
            AssignedTo = new ApplicationUser
            {
                Id = assignedToId,
                FullName = "Tran Thi B"
            }
        };

        _leadReadRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([lead]);
        var handler = new GetLeadsQueryHandler(_leadReadRepoMock.Object);

        var result = await handler.Handle(new GetLeadsQuery(), CancellationToken.None).ConfigureAwait(true);

        result.Should().HaveCount(1);
        var response = result[0];
        response.InterestedVehicle.Should().Be(lead.InterestedVehicle);
        response.AddressDetail.Should().Be(lead.AddressDetail);
        response.Ward.Should().Be(lead.Ward);
        response.District.Should().Be(lead.District);
        response.Province.Should().Be(lead.Province);
        response.Gender.Should().Be(lead.Gender);
        response.Birthday.Should().Be(lead.Birthday);
        response.IdentificationNumber.Should().Be(lead.IdentificationNumber);
        response.IsVerified.Should().Be(lead.IsVerified);
        response.Tier.Should().Be(lead.Tier);
        response.Points.Should().Be(lead.Points);
        response.AssignedToId.Should().Be(lead.AssignedToId);
        response.AssignedToName.Should().Be(lead.AssignedTo.FullName);
    }

    [Fact(DisplayName = "LEAD_017 - Khong tao nhac nho don hang ket trang thai cho dau ra da hoan tat")]
    public async Task GetCustomerProfile360_ShouldNotCreateStalledReminderForTerminalOutputs()
    {
        var lead = new LeadEntity
        {
            Id = 1,
            FullName = "Nguyen Van A",
            PhoneNumber = "0909123456"
        };
        var output = new Domain.Entities.Output
        {
            Id = 10,
            LeadId = lead.Id,
            StatusId = OrderStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-45),
            LastStatusChangedAt = DateTimeOffset.UtcNow.AddDays(-40)
        };

        _leadReadRepoMock.Setup(x => x.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _outputReadRepoMock.Setup(x => x.GetByLeadIdAsync(lead.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([output]);
        _plateDossierReadRepoMock.Setup(x => x.GetPagedAsync<PlateDossierResponse>(
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode>(),
                It.IsAny<Expression<Func<PlateDossier, bool>>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<PlateDossierResponse>([], 0, 1, int.MaxValue));
        _repairOrderReadRepoMock.Setup(x => x.GetByCustomerPhoneAsync(lead.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _vehicleReadRepoMock.Setup(x => x.GetByLeadIdAsync(lead.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = new GetCustomerProfile360QueryHandler(
            _leadReadRepoMock.Object,
            _outputReadRepoMock.Object,
            _plateDossierReadRepoMock.Object,
            _repairOrderReadRepoMock.Object,
            _vehicleReadRepoMock.Object);

        var result = await handler.Handle(new GetCustomerProfile360Query(lead.Id), CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        result.Value.CareReminders.Should().NotContain(r => r.Type == "stalled_order");
    }

    [Fact(DisplayName = "LEAD_021 - Logic xử lý mô tả khi loại đặt lịch là Lái thử")]
    public async Task CreateBooking_TestDriveType_DescriptionUsesFriendlyName()
    {
        var command = new CreateBookingCommand
        {
            PhoneNumber = "0909000021",
            FullName = "User",
            BookingType = "TestDrive",
            Location = "Showroom"
        };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadEntity?)null);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _leadActivityInsertRepoMock.Verify(
            x => x.Add(It.Is<LeadActivity>(a => a.Description.Contains("Lái thử"))),
            Times.Once);
    }

    [Fact(DisplayName = "LEAD_022 - Logic xử lý mô tả cho các loại đặt lịch khác")]
    public async Task CreateBooking_OtherType_DescriptionUsesOriginalType()
    {
        var command = new CreateBookingCommand
        {
            PhoneNumber = "0909000022",
            FullName = "User",
            BookingType = "Consulting",
            Location = "Showroom"
        };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadEntity?)null);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _leadActivityInsertRepoMock.Verify(
            x => x.Add(It.Is<LeadActivity>(a => a.Description.Contains("Consulting"))),
            Times.Once);
    }

    [Fact(DisplayName = "LEAD_027 - Gán điểm khởi tạo cho khách hàng mới")]
    public async Task CreateBooking_NewLead_SetsInitialScore30()
    {
        var command = new CreateBookingCommand { PhoneNumber = "0909000001", FullName = "New User" };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadEntity?)null);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<LeadEntity>(l => l.Score == 30)), Times.Once);
    }

    [Fact(DisplayName = "LEAD_028 - Cộng dồn điểm khi khách hàng cũ đặt lịch")]
    public async Task CreateBooking_ExistingLead_Adds30Points()
    {
        var command = new CreateBookingCommand { PhoneNumber = "0909000002", FullName = "Existing User" };
        var existingLead = new LeadEntity { Id = 1, PhoneNumber = "0909000002", Score = 30 };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLead);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        existingLead.Score.Should().Be(60);
        _leadUpdateRepoMock.Verify(x => x.Update(existingLead), Times.Once);
    }

    [Theory(DisplayName = "LEAD_032 - Cộng điểm không phụ thuộc vào địa điểm đặt lịch")]
    [InlineData("Showroom")]
    [InlineData("CustomerAddress")]
    public async Task CreateBooking_DifferentLocations_StillAdds30Points(string location)
    {
        var command = new CreateBookingCommand { PhoneNumber = "0909000003", FullName = "User", Location = location };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadEntity?)null);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<LeadEntity>(l => l.Score == 30)), Times.Once);
    }

    [Theory(DisplayName = "LEAD_034 - Kiểm tra Score khi đặt lịch với loại Booking khác nhau")]
    [InlineData("TestDrive")]
    [InlineData("Consulting")]
    public async Task CreateBooking_DifferentTypes_StillAdds30Points(string bookingType)
    {
        var command = new CreateBookingCommand
        {
            PhoneNumber = "0909000004",
            FullName = "User",
            BookingType = bookingType
        };
        _leadReadRepoMock.Setup(x => x.GetByPhoneNumberAsync(command.PhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadEntity?)null);
        var handler = new CreateBookingCommandHandler(
            _bookingInsertRepoMock.Object,
            _leadReadRepoMock.Object,
            _leadInsertRepoMock.Object,
            _leadUpdateRepoMock.Object,
            _leadActivityInsertRepoMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _leadInsertRepoMock.Verify(x => x.Add(It.Is<LeadEntity>(l => l.Score == 30)), Times.Once);
    }

    [Fact(DisplayName = "LEAD_049 - Logic tích lũy điểm qua chuỗi hành động")]
    public async Task LEAD_049_Accumulate_Score_Sequence()
    {
        var lead = new LeadEntity { Id = 1, Score = 0 };
        _leadReadRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        var handler = new AddLeadActivityCommandHandler(_leadUpdateRepoMock.Object, _leadReadRepoMock.Object);
        var actions = new[]
        {
            new { Type = "TestDrive", Desc = "Khách lái thử xe" },
            new { Type = "Call", Desc = "Missed call from lead" },
            new { Type = "TestDrive", Desc = "Khách lái thử xe lần 2" }
        };
        foreach (var action in actions)
        {
            var command = new AddLeadActivityCommand(1, action.Type, action.Desc);
            await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        }
        lead.Score.Should().Be(30);
    }

    [Fact(DisplayName = "LEAD_052 - Kiểm tra giới hạn điểm dưới (Score Floor)")]
    public async Task LEAD_052_Score_Floor_Limit()
    {
        var lead = new LeadEntity { Id = 1, Score = 5 };
        _leadReadRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        var handler = new AddLeadActivityCommandHandler(_leadUpdateRepoMock.Object, _leadReadRepoMock.Object);
        for (int i = 0; i < 5; i++)
        {
            var command = new AddLeadActivityCommand(1, "Phone Call", "Khách không nghe máy (missed)");
            await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        }
        lead.Score.Should().Be(0);
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}

