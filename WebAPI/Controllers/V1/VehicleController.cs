using Application.ApiContracts.Vehicle.Responses;
using Application.Features.Vehicles.Commands.CreateVehicle;
using Application.Features.Vehicles.Commands.TransferOwnership;
using Application.Features.Vehicles.Commands.UpdateLicensePlate;
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
    /// <param name="id">The vehicle ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy chi tiết xe của khách hàng")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return Ok(new VehicleResponse { Id = id });
    }

    /// <summary>
    /// Lấy danh sách xe của khách hàng
    /// </summary>
    /// <param name="sieveModel">Sieve model for filtering, sorting and pagination.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Danh sách xe</returns>
    [HttpGet]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy danh sách xe của khách hàng")]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVehiclesQuery { SieveModel = sieveModel }, cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới tài sản xe
    /// </summary>
    /// <param name="command">The create vehicle command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateVehicleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result);
    }

    /// <summary>
    /// Cập nhật biển số xe
    /// </summary>
    /// <param name="id">The vehicle ID.</param>
    /// <param name="command">The update license plate command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPatch("{id:int}/license-plate")]
    [Authorize]
    public async Task<IActionResult> UpdateLicensePlateAsync(
        int id,
        [FromBody] UpdateLicensePlateCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Chuyển quyền sở hữu xe
    /// </summary>
    /// <param name="id">The vehicle ID.</param>
    /// <param name="command">The transfer ownership command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost("{id:int}/transfer")]
    [Authorize]
    public async Task<IActionResult> TransferOwnershipAsync(
        int id,
        [FromBody] TransferOwnershipCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
