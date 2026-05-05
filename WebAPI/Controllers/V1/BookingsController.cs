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
/// Controller quản lý đặt lịch lái thử.
/// </summary>
/// <param name="sender"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý đặt lịch lái thử (Booking)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BookingsController(ISender sender) : ApiController
{
    /// <summary>
    /// Tạo yêu cầu đặt lịch lái thử mới.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAsync(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách yêu cầu đặt lịch lái thử.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xác nhận yêu cầu đặt lịch lái thử.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmAsync(ConfirmBookingCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
