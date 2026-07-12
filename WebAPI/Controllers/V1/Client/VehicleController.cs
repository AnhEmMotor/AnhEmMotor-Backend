using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1.Client
{
    [ApiController]
    [Route("api/v1/client/vehicles")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMyVehicles(CancellationToken cancellationToken)
        {
            return Ok(new { message = "Endpoint temporarily unavailable" });
        }

        [HttpPost("register-odo")]
        public async Task<IActionResult> RegisterOdo(CancellationToken cancellationToken)
        {
            return Ok(new { message = "Endpoint temporarily unavailable" });
        }
    }
}
