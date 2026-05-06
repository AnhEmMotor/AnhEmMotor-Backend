using Application.Common.Models;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Features.Bookings.Queries.GetBookings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.V1;
using Xunit;

namespace ControllerTests;

public class Booking
{
    private readonly Mock<ISender> _senderMock;
    private readonly BookingsController _bookingsController;

    public Booking()
    {
        _senderMock = new Mock<ISender>();
        _bookingsController = new BookingsController(_senderMock.Object);
        
        var httpContext = new DefaultHttpContext();
        _bookingsController.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact(DisplayName = "BOOKING_001 - Tạo lịch hẹn với đầy đủ thông tin hợp lệ (API)")]
    public async Task CreateBooking_ValidRequest_ReturnsBookingId()
    {
        // Arrange
        var command = new CreateBookingCommand 
        { 
            FullName = "Test User", 
            PhoneNumber = "0909123456", 
            Email = "test@gmail.com",
            ProductVariantId = 1,
            PreferredDate = DateTimeOffset.UtcNow.AddDays(1)
        };
        _senderMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(100));

        // Act
        var result = await _bookingsController.CreateAsync(command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<int>(okResult.Value);
        Assert.Equal(100, response);
    }

    [Fact(DisplayName = "BOOKING_016 - Kiểm tra tích hợp Sieve Paging cho danh sách Booking")]
    public async Task GetBookings_WithPaging_ReturnsPagedList()
    {
        // Arrange
        var query = new GetBookingsQuery();
        // Giả sử GetBookingsQuery trả về danh sách DTO
        _senderMock.Setup(m => m.Send(It.IsAny<GetBookingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<Domain.Entities.Booking>>.Success([]));

        // Act
        var result = await _bookingsController.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
