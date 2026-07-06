using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Features.WorkshopPayments.Queries;
using Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentStatistics;
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
[SwaggerTag("Quản lý phiếu thu xưởng (Workshop Payment)")]
[Route("api/v{version:apiVersion}/WorkshopPayments")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class WorkshopPaymentsController(IMediator mediator) : ApiController
{
    [HttpGet]
    [HasPermission(Permissions.Factory.RepairOrderManagement.View)]
    [ProducesResponseType(typeof(PagedResult<WorkshopPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWorkshopPaymentsListQuery { Sieve = sieveModel }, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
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
    [ProducesResponseType(typeof(WorkshopPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetailAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetWorkshopPaymentDetailQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
