using Application.ApiContracts.Vehicle.Requests;
using Application.ApiContracts.Vehicle.Responses;
using Application.Features.Client.Vehicles.Commands.RegisterCustomerVehicle;
using Application.Features.Client.Vehicles.Queries;
using Application.Features.Client.Vehicles.Queries.GetCustomerVehicleDetail;
using Application.Features.Client.Vehicles.Queries.GetCustomerVehicleHistory;
using Domain.Primitives;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebAPI.Controllers.V1.Client;

/// <summary>
/// Quản lý xe của khách hàng (Client Portal).
/// </summary>
[ApiController]
[Route("api/v1/client/vehicles")]
[Authorize]
public class VehicleController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách xe đã đăng ký của khách hàng đang đăng nhập.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VehicleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyVehicles(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            User.FindFirst("sub")?.Value ??
            User.Identity?.Name ??
            string.Empty;
        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return BadRequest(new { message = "Invalid user identifier" });
        }
        var result = await mediator.Send(
            new GetMyVehiclesQuery { UserId = userId, SieveModel = sieveModel },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết xe của khách hàng.
    /// </summary>
    [HttpGet("{id}/detail")]
    [ProducesResponseType(typeof(VehicleDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicleDetail([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            User.FindFirst("sub")?.Value ??
            User.Identity?.Name ??
            string.Empty;
        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return BadRequest(new { message = "Invalid user identifier" });
        }
        var result = await mediator.Send(
            new GetCustomerVehicleDetailQuery { UserId = userId, VehicleId = id },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy lịch sử mua hàng và bảo hành của xe.
    /// </summary>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(CustomerVehicleHistoryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicleHistory([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            User.FindFirst("sub")?.Value ??
            User.Identity?.Name ??
            string.Empty;
        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return BadRequest(new { message = "Invalid user identifier" });
        }
        var result = await mediator.Send(
            new GetCustomerVehicleHistoryQuery { UserId = userId, VehicleId = id },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Đăng ký xe mới cho khách hàng.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterVehicle(
        [FromBody] RegisterCustomerVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            User.FindFirst("sub")?.Value ??
            User.Identity?.Name ??
            string.Empty;
        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return BadRequest(new { message = "Invalid user identifier" });
        }
        var result = await mediator.Send(
            new RegisterCustomerVehicleCommand
            {
                UserId = userId,
                LicensePlate = request.LicensePlate?.Trim() ?? string.Empty,
                VinNumber = request.Vin?.Trim() ?? string.Empty,
                EngineNumber = request.EngineNumber?.Trim() ?? string.Empty,
                CurrentOdo = request.CurrentOdo,
            },
            cancellationToken);
        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? result.Error;
            return BadRequest(new { message = error?.Message ?? "Đăng ký xe thất bại" });
        }
        return Ok(result);
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
