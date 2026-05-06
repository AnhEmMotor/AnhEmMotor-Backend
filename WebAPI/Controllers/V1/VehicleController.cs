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
}
