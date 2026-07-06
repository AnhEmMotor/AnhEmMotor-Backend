using Application.Common.Models;
using Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentStatistics;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý phiếu thu xưởng (Workshop Payment)")]
[Route("api/v{version:apiVersion}/WorkshopPayments")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class WorkshopPaymentsController(IMediator mediator) : ApiController
{
    [HttpGet]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync(CancellationToken cancellationToken)
    {
        return Ok(new { items = Array.Empty<object>(), totalCount = 0 });
    }

    [HttpGet("stats")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(typeof(WorkshopPaymentStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var query = new GetWorkshopPaymentStatisticsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetailAsync(int id, CancellationToken cancellationToken)
    {
        return Ok(new { message = "Not implemented yet" });
    }
}
