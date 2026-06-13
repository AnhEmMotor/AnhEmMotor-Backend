using Application.Features.Statistical.Queries.GetWorkshopDashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers.V1;

[ApiController]
[Route("api/workshop/dashboard")]
public class WorkshopDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkshopDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(
        [FromQuery] DateTimeOffset fromDate,
        [FromQuery] DateTimeOffset toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkshopDashboardQuery(fromDate, toDate);
        var result = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
