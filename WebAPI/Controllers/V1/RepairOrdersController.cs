using Application.Common.Models;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý phiếu sửa chữa (Repair Order)")]
[Route("api/v{version:apiVersion}/RepairOrders")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class RepairOrdersController : ApiController
{
  [HttpGet]
  [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken)
  {
    return Ok(new { items = Array.Empty<object>(), totalCount = 0 });
  }

  [HttpGet("{id:int}")]
  [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> GetDetailAsync(int id, CancellationToken cancellationToken)
  {
    return Ok(new { message = "Not implemented yet" });
  }
}
