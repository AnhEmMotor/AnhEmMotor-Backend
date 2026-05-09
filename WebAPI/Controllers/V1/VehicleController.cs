using Application.ApiContracts.Maintenance.Responses;
using Application.Features.Vehicles.Commands.CreateVehicle;
using Application.Features.Vehicles.Commands.TransferOwnership;
using Application.Features.Vehicles.Commands.UpdateLicensePlate;
using Application.Features.Vehicles.Queries.GetVehicles;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy chi tiết xe của khách hàng")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(new Application.ApiContracts.Maintenance.Responses.VehicleResponse { Id = id });
    }

    /// <summary>
    /// Lấy danh sách xe của khách hàng
    /// </summary>
    /// <param name="search">Từ khóa tìm kiếm (Biển số, VIN, Tên khách)</param>
    /// <returns>Danh sách xe</returns>
    [HttpGet]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy danh sách xe của khách hàng")]
    public async Task<IActionResult> GetList([FromQuery] string? search)
    {
        var result = await mediator.Send(new GetVehiclesQuery { Search = search });
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới tài sản xe
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Application.ApiContracts.Maintenance.Responses.VehicleResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateVehicleCommand command)
    {
        var result = await mediator.Send(command);
        return HandleCreated(result, null, null);
    }

    /// <summary>
    /// Cập nhật biển số xe
    /// </summary>
    [HttpPatch("{id:int}/license-plate")]
    [Authorize]
    public async Task<IActionResult> UpdateLicensePlateAsync(int id, [FromBody] UpdateLicensePlateCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Chuyển quyền sở hữu xe
    /// </summary>
    [HttpPost("{id:int}/transfer")]
    [Authorize]
    public async Task<IActionResult> TransferOwnershipAsync(int id, [FromBody] TransferOwnershipCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);
        return HandleResult(result);
    }
}
