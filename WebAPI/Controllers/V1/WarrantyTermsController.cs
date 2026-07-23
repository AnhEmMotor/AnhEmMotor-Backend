using Application.ApiContracts.Admin.Warranty;
using Application.Common.Models;
using Application.Features.WarrantyTerms.Commands.CreateWarrantyTerm;
using Application.Features.WarrantyTerms.Commands.DeleteWarrantyTerm;
using Application.Features.WarrantyTerms.Commands.UpdateWarrantyTerm;
using Application.Features.WarrantyTerms.Queries.GetWarrantyTermById;
using Application.Features.WarrantyTerms.Queries.GetWarrantyTermsList;
using Application.Features.WarrantyTerms.Queries.GetWarrantyTermStatistics;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý danh mục bảo hành theo hãng")]
[Route("api/v{version:apiVersion}/WarrantyTerms")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class WarrantyTermsController(ISender sender) : ApiController
{
    [HttpGet]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(typeof(PagedResult<WarrantyTermResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync([FromQuery] SieveModel sieve, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWarrantyTermsListQuery { SieveModel = sieve }, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(typeof(WarrantyTermResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetailAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWarrantyTermByIdQuery { Id = id }, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("statistics")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(typeof(WarrantyTermStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWarrantyTermStatisticsQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Factory.RepairOrderManagement.Create)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateWarrantyTermCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
            return HandleResult(result);
        return CreatedAtAction("GetDetail", new { id = result.Value, version = "1.0" }, result.Value);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.AssignTechnician)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateWarrantyTermCommand command,
        CancellationToken cancellationToken)
    {
        var merged = command with { Id = id };
        var result = await sender.Send(merged, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.AssignTechnician)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteWarrantyTermCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
