using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using AnhEmMotor.Application.ApiContracts.Client.Vehicles;
using AnhEmMotor.Application.Features.Client.Vehicles;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AnhEmMotor.WebAPI.Controllers.V1.Client
{
    [ApiController]
    [Route("api/v1/client/vehicles")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IMediator _mediator;
        public VehicleController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetMyVehicles()
        {
            var result = await _mediator.Send(new GetMyVehiclesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleDetail(int id)
        {
            var result = await _mediator.Send(new GetVehicleDetailQuery(id));
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("register-odo")]
        public async Task<IActionResult> UpdateOdo([FromBody] UpdateOdoRequest request, [FromQuery] int vehicleId)
        {
            var result = await _mediator.Send(new UpdateOdoCommand(vehicleId, request.NewOdo));
            return result ? Ok() : BadRequest();
        }
    }
}