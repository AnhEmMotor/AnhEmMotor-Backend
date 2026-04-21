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

[ApiVersion("1.0")]
[SwaggerTag("Quản lý đặt lịch lái thử (Booking)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BookingsController(ISender sender) : ApiController
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(CreateBookingCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await sender.Send(new GetBookingsQuery());
        return HandleResult(result);
    }

    [HttpPost("confirm")]
    [Authorize]
    public async Task<IActionResult> Confirm(ConfirmBookingCommand command)
    {
        var result = await sender.Send(command);
        return HandleResult(result);
    }
}
