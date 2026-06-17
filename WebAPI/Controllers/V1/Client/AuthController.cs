using Microsoft.AspNetCore.Mvc;
using MediatR;
using AnhEmMotor.Application.ApiContracts.Client.Auth;
using AnhEmMotor.Application.Features.Client.Auth;
using System.Threading.Tasks;

namespace AnhEmMotor.WebAPI.Controllers.V1.Client
{
    [ApiController]
    [Route("api/v1/client/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator) => _mediator = mediator;

        [HttpPost("login-phone")]
        public async Task<IActionResult> LoginPhone([FromBody] LoginPhoneRequest request)
        {
            var result = await _mediator.Send(new LoginPhoneCommand(request.PhoneNumber));
            return result ? Ok(new { message = "OTP sent successfully" }) : BadRequest("Failed to send OTP");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _mediator.Send(new VerifyOtpCommand(request.PhoneNumber, request.OtpCode));
            return result != null ? Ok(result) : BadRequest("Invalid OTP");
        }
    }
}