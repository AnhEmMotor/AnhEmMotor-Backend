using Application.ApiContracts.Client.Profile;
using Application.Features.Client.Profile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1.Client
{
    [ApiController]
    [Route("api/v1/client/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfileController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _mediator.Send(new GetProfileQuery());
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var result = await _mediator.Send(new UpdateProfileCommand(request));
            return result ? Ok() : BadRequest();
        }
    }
}
