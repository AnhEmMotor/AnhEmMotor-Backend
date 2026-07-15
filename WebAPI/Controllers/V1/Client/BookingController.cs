using Application.ApiContracts.Client.Bookings;
using Application.Features.Client.Bookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers.V1.Client;

/// <summary>
/// Quản lý đặt lịch dịch vụ (Client Portal).
/// </summary>
[ApiController]
[Route("api/v1/client/bookings")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lấy danh sách khung giờ còn trống để đặt lịch theo ngày.
    /// </summary>
    /// <param name="date">Ngày cần xem khung giờ trống.</param>
    [HttpGet("available-slots")]
    public async Task<IActionResult> GetSlots([FromQuery] DateTime date)
    {
        var result = await _mediator.Send(new GetAvailableSlotsQuery(date));
        return Ok(result);
    }

    /// <summary>
    /// Tạo lịch đặt dịch vụ mới.
    /// </summary>
    /// <param name="request">Thông tin đặt lịch (ngày, khung giờ, dịch vụ).</param>
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var id = await _mediator.Send(new CreateBookingCommand(request));
        return Ok(new { BookingId = id });
    }

    /// <summary>
    /// Lấy lịch sử đặt lịch của khách hàng đang đăng nhập.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _mediator.Send(new GetBookingHistoryQuery());
        return Ok(result);
    }

    /// <summary>
    /// Hủy lịch đặt dịch vụ.
    /// </summary>
    /// <param name="id">ID của lịch đặt cần hủy.</param>
    /// <param name="request">Lý do hủy.</param>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingRequest request)
    {
        var result = await _mediator.Send(new CancelBookingCommand(id, request.Reason));
        return result ? Ok() : BadRequest();
    }
}
