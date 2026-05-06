using Application.Features.Bookings.Commands.ConfirmBooking;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Features.Bookings.Queries.GetBookings;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý đặt lịch lái thử (Booking)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý đặt lịch lái thử (Booking)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BookingsController(ISender sender) : ApiController
{
    /// <summary>
    /// Tạo mới một lịch hẹn lái thử
    /// </summary>
    /// <param name="command">Dữ liệu đặt lịch</param>
    /// <returns>Kết quả tạo mới</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(CreateBookingCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách tất cả các lịch đặt
    /// </summary>
    /// <returns>Danh sách đặt lịch</returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await sender.Send(new GetBookingsQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Xác nhận lịch đặt (Chốt lịch)
    /// </summary>
    /// <param name="command">Dữ liệu xác nhận</param>
    /// <returns>Kết quả xác nhận</returns>
    [HttpPost("confirm")]
    [Authorize]
    public async Task<IActionResult> Confirm(ConfirmBookingCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }
}
