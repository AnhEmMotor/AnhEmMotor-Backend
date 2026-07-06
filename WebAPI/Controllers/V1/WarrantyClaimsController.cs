using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Features.WarrantyClaims.Commands;
using Application.Features.WarrantyClaims.Queries;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý khiếu nại bảo hành (Warranty Claim)")]
[Route("api/v{version:apiVersion}/WarrantyClaims")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class WarrantyClaimsController(ISender sender) : ApiController
{
 [HttpGet]
 [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
 [ProducesResponseType(typeof(PagedResult<WarrantyClaimResponse>), StatusCodes.Status200OK)]
 public async Task<IActionResult> GetListAsync([FromQuery] SieveModel sieve, CancellationToken cancellationToken)
 {
  var result = await sender.Send(new GetWarrantyClaimsListQuery { Sieve = sieve }, cancellationToken);
  return HandleResult(result);
 }

 [HttpGet("{id:int}")]
 [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
 [ProducesResponseType(typeof(WarrantyClaimResponse), StatusCodes.Status200OK)]
 public async Task<IActionResult> GetDetailAsync(int id, CancellationToken cancellationToken)
 {
  var result = await sender.Send(new GetWarrantyClaimDetailQuery { Id = id }, cancellationToken);
  return HandleResult(result);
 }

 [HttpGet("vehicle/{vehicleId:int}/history")]
 [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
 [ProducesResponseType(typeof(IEnumerable<WarrantyHistoryResponse>), StatusCodes.Status200OK)]
 public async Task<IActionResult> GetHistoryByVehicleAsync(int vehicleId, CancellationToken cancellationToken)
 {
  var result = await sender.Send(new GetWarrantyHistoryQuery(vehicleId), cancellationToken);
  return HandleResult(result);
 }

 [HttpPost]
 [HasPermission(Permissions.Factory.RepairOrderManagement.Create)]
 [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
 public async Task<IActionResult> CreateAsync([FromBody] CreateWarrantyClaimCommand command, CancellationToken cancellationToken)
 {
  var result = await sender.Send(command, cancellationToken);
  if (!result.IsSuccess) return HandleResult(result);
  return CreatedAtAction(nameof(GetDetailAsync), new { id = result.Value, version = "1.0" }, result.Value);
 }

 [HttpPatch("{id:int}/status")]
 [HasPermission(Permissions.Factory.RepairOrderManagement.AssignTechnician)]
 [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
 public async Task<IActionResult> UpdateStatusAsync(int id, [FromBody] UpdateWarrantyClaimCommand command, CancellationToken cancellationToken)
 {
  var merged = command with { Id = id };
  var result = await sender.Send(merged, cancellationToken);
  return HandleResult(result);
 }
}
