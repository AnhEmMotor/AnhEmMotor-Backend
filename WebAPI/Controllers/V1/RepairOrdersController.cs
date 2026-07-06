using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Features.RepairOrders.Queries;
using Application.Features.RepairOrders.Commands;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý phiếu sửa chữa (Repair Order)")]
[Route("api/v{version:apiVersion}/RepairOrders")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class RepairOrdersController(ISender sender) : ApiController
{
  [HttpGet]
  [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
  [ProducesResponseType(typeof(PagedResult<RepairOrderResponse>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetListAsync([FromQuery] GetRepairOrdersListQuery query, CancellationToken cancellationToken)
  {
    var result = await sender.Send(query, cancellationToken);
    return HandleResult(result);
  }

  [HttpGet("{id:int}")]
  [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
  [ProducesResponseType(typeof(RepairOrderResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetDetailAsync(int id, CancellationToken cancellationToken)
  {
    var result = await sender.Send(new GetRepairOrderDetailQuery { Id = id }, cancellationToken);
    return HandleResult(result);
  }

  [HttpPut("{id:int}")]
  [HasPermission(Permissions.Factory.RepairOrderManagement.AssignTechnician)]
  [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
  public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateRepairOrderCommand command, CancellationToken cancellationToken)
  {
    if (id != command.Id) return BadRequest();
    var result = await sender.Send(command, cancellationToken);
    return HandleResult(result);
  }

  [HttpPost("issue-parts")]
  [HasPermission(Permissions.Factory.RepairOrderManagement.StartRepair)]
  [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
  public async Task<IActionResult> IssuePartsAsync([FromBody] IssuePartsCommand command, CancellationToken cancellationToken)
  {
    var result = await sender.Send(command, cancellationToken);
    return HandleResult(result);
  }

  [HttpPost("complete")]
  [HasPermission(Permissions.Factory.RepairOrderManagement.Complete)]
  [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
  public async Task<IActionResult> CompleteAsync([FromBody] CompleteRepairOrderCommand command, CancellationToken cancellationToken)
  {
    var result = await sender.Send(command, cancellationToken);
    return HandleResult(result);
  }
}
