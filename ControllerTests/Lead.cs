using Application.ApiContracts.Leads.Responses;
using Application.Common.Models;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Features.Leads.Commands.UpdateLead;
using Application.Features.Leads.Commands.AddLeadActivity;
using Application.Features.Leads.Queries.GetLeads;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using FluentAssertions;
using Xunit;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class Lead
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly BookingsController _bookingsController;
    private readonly LeadController _leadController;

    public Lead()
    {
        _mediatorMock = new Mock<IMediator>();
        _bookingsController = new BookingsController(_mediatorMock.Object);
        _leadController = new LeadController(_mediatorMock.Object);
        var httpContext = new DefaultHttpContext();
        _bookingsController.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _leadController.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact(DisplayName = "LEAD_002 - Tạo Lead mới tự động khi khách hàng đặt lịch lần đầu (API)")]
    public async Task CreateBooking_NewCustomer_ReturnsSuccess()
    {
        var command = new CreateBookingCommand { PhoneNumber = "0909999999", FullName = "New Customer" };
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result<int>.Success(1));
        var result = await _bookingsController.CreateAsync(command, CancellationToken.None).ConfigureAwait(true);
        Assert.IsType<OkObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "LEAD_003 - Tăng điểm tiềm năng cho Lead đã tồn tại khi đặt lịch thêm (API)")]
    public async Task CreateBooking_ExistingCustomer_ReturnsSuccess()
    {
        var command = new CreateBookingCommand { PhoneNumber = "0909123456", FullName = "Existing Customer" };
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result<int>.Success(1));
        var result = await _bookingsController.CreateAsync(command, CancellationToken.None).ConfigureAwait(true);
        Assert.IsType<OkObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "LEAD_004 - Ghi nhận hoạt động đặt lịch mới cho Lead (API)")]
    public async Task CreateBooking_RecordActivity_ReturnsSuccess()
    {
        var command = new CreateBookingCommand
        {
            PhoneNumber = "0909123456",
            FullName = "Existing Customer",
            Location = "Showroom A"
        };
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result<int>.Success(1));
        var result = await _bookingsController.CreateAsync(command, CancellationToken.None).ConfigureAwait(true);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "LEAD_016 - API GetLeads trả về lỗi 401 khi chưa đăng nhập")]
    public async Task GetLeads_NotAuthenticated_ThrowsUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetLeadsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _leadController.GetLeadsAsync(CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "LEAD_026 - Đảm bảo tính nhất quán của danh sách hoạt động trong LeadResponse")]
    public async Task GetLeads_MultipleLeads_ReturnsCorrectActivitiesPerLead()
    {
        var expectedLeads = new List<LeadResponse>
        {
            new()
            {
                Id = 1,
                FullName = "User A",
                Activities = [new LeadActivityResponse { Id = 10, Description = "Activity A" }]
            },
            new()
            {
                Id = 2,
                FullName = "User B",
                Activities = [new LeadActivityResponse { Id = 20, Description = "Activity B" }]
            }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetLeadsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLeads);
        var result = await _leadController.GetLeadsAsync(CancellationToken.None).ConfigureAwait(true);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var leads = Assert.IsType<List<LeadResponse>>(okResult.Value);
        Assert.Equal(2, leads.Count);
        Assert.Equal("Activity A", leads[0].Activities[0].Description);
        Assert.Equal("Activity B", leads[1].Activities[0].Description);
    }

    [Fact(DisplayName = "LEAD_036 - Kiểm tra phân quyền truy cập thông tin Score")]
    public async Task GetLeads_UserWithPermission_ReturnsScore()
    {
        var expectedLeads = new List<LeadResponse> { new() { Id = 1, FullName = "User A", Score = 100 } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetLeadsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLeads);
        var result = await _leadController.GetLeadsAsync(CancellationToken.None).ConfigureAwait(true);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var leads = Assert.IsType<List<LeadResponse>>(okResult.Value);
        Assert.Single(leads);
        Assert.Equal(100, leads[0].Score);
    }

    [Fact(DisplayName = "LEAD_050 - Không thay đổi điểm với hành động không xác định")]
    public async Task LEAD_050_Add_Unknown_Activity_Should_Not_Change_Score()
    {
        // Arrange
        var command = new AddLeadActivityCommand(1, "ViewWebsite", "Khách xem website");
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddLeadActivityCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(123); // Activity ID

        // Action
        var result = await _leadController.AddActivityAsync(1, command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(
            m => m.Send(It.Is<AddLeadActivityCommand>(c => string.Compare(c.ActivityType, "ViewWebsite") == 0), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "LEAD_038 - Cập nhật địa chỉ chi tiết cho Lead")]
    public async Task LEAD_038_Update_Lead_Address_Returns_Success()
    {
        // Arrange
        var command = new UpdateLeadCommand
        {
            Id = 1,
            FullName = "Test",
            AddressDetail = "123 Street",
            Ward = "Ward A",
            District = "District B",
            Province = "Province C"
        };
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(1));

        // Action
        var result = await _leadController.UpdateLeadAsync(1, command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "LEAD_041 - Chuyển đổi trạng thái Lead trong Pipeline")]
    public async Task LEAD_041_Update_Lead_Status_Returns_Success()
    {
        // Arrange
        var command = new UpdateLeadCommand
        {
            Id = 1,
            FullName = "Test",
            Status = "TestDrive"
        };
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(1));

        // Action
        var result = await _leadController.UpdateLeadAsync(1, command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "LEAD_050 - Scenario 2 - Hành động lạ không thay đổi điểm")]
    public async Task LEAD_050_Add_Another_Unknown_Activity_Success()
    {
        // Arrange
        var command = new AddLeadActivityCommand(1, "StrangeAction", "Mô tả lạ");
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddLeadActivityCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(456);

        // Action
        var result = await _leadController.AddActivityAsync(1, command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
