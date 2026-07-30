using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1.Client;

/// <summary>
/// Quản lý xe của khách hàng (Client Portal).
/// </summary>
[ApiController]
[Route("api/v1/client/vehicles")]
[Authorize]
public class VehicleController : ControllerBase
{
    /// <summary>
    /// Lấy danh sách xe đã đăng ký của khách hàng đang đăng nhập.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyVehicles(CancellationToken cancellationToken)
    {
        return Ok(new { message = "Endpoint temporarily unavailable" });
    }

    /// <summary>
    /// Đăng ký xe mới cho khách hàng.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RegisterVehicle([FromBody] object request, CancellationToken cancellationToken)
    {
        // TODO: Implement actual logic
        return Ok(new { message = "Vehicle registered successfully (mock)" });
    }

    /// <summary>
    /// Đăng ký số km odometer cho xe của khách hàng.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost("register-odo")]
    public async Task<IActionResult> RegisterOdo(CancellationToken cancellationToken)
    {
        return Ok(new { message = "Endpoint temporarily unavailable" });
    }
}
