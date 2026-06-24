using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.ApiContracts.Client.Bookings;
using Application.Features.Client.Bookings;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace AnhEmMotor.WebAPI.Controllers.V1.Client
{
    [ApiController]
    [Route("api/v1/client/bookings")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;
        public BookingController(IMediator mediator) => _mediator = mediator;

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetSlots([FromQuery] DateTime date)
        {
            var result = await _mediator.Send(new GetAvailableSlotsQuery(date));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            var id = await _mediator.Send(new CreateBookingCommand(request));
            return Ok(new { BookingId = id });
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var result = await _mediator.Send(new GetBookingHistoryQuery());
            return Ok(result);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingRequest request)
        {
            var result = await _mediator.Send(new CancelBookingCommand(id, request.Reason));
            return result ? Ok() : BadRequest();
        }
    }
}
