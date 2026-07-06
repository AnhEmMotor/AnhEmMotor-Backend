using Application.ApiContracts.Vehicle.Responses;
using Application.Features.Vehicles.Commands.CreateVehicle;
using Application.Features.Vehicles.Commands.TransferOwnership;
using Application.Features.Vehicles.Queries.GetVehiclePortfolio;
using Application.Features.Vehicles.Queries.GetVehicles;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý Tài sản xe của khách hàng
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý Tài sản xe của khách hàng")]
[Route("api/v{version:apiVersion}/[controller]")]
public class VehicleController(IMediator mediator) : ApiController
{
/// <summary>
/// Lấy chi tiết xe của khách hàng
/// </summary>
[HttpGet("{id:int}")]
[Authorize]
[SwaggerOperation(Summary = "Lấy chi tiết xe của khách hàng")]
public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
{
cancellationToken.ThrowIfCancellationRequested();
return Ok(new VehicleResponse { Id = id });
}

/// <summary>
/// Lấy danh sách xe của khách hàng
/// </summary>
[HttpGet]
[Authorize]
[SwaggerOperation(Summary = "Lấy danh sách xe của khách hàng")]
public async Task<IActionResult> GetListAsync(
[FromQuery] SieveModel sieveModel,
CancellationToken cancellationToken)
{
var result = await mediator.Send(new GetVehiclesQuery { SieveModel = sieveModel }, cancellationToken)
.ConfigureAwait(false);
return HandleResult(result);
}

/// <summary>
/// Tạo mới tài sản xe
/// </summary>
[HttpPost]
[Authorize]
[ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
public async Task<IActionResult> CreateAsync(
[FromBody] CreateVehicleCommand command,
CancellationToken cancellationToken)
{
var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
return HandleCreated(result);
}

/// <summary>
/// Chuyển quyền sở hữu xe
/// </summary>
[HttpPost("{id:int}/transfer")]
[Authorize]
public async Task<IActionResult> TransferOwnershipAsync(
int id,
[FromBody] TransferOwnershipCommand command,
CancellationToken cancellationToken)
{
command.Id = id;
var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
return HandleResult(result);
}

/// <summary>
/// Tra cứu hồ sơ xe theo VIN, biển số, số điện thoại
/// </summary>
[HttpGet("portfolio")]
[Authorize]
[SwaggerOperation(Summary = "Tra cứu hồ sơ xe")]
public async Task<IActionResult> GetPortfolioAsync(
[FromQuery] string query,
[FromQuery] string queryType,
[FromQuery] int page = 1,
[FromQuery] int pageSize = 5,
CancellationToken cancellationToken = default)
{
var result = await mediator.Send(
new GetVehiclePortfolioQuery(query ?? "", queryType ?? "auto", page, pageSize), cancellationToken).ConfigureAwait(false);
return HandleResult(result);
}
}
