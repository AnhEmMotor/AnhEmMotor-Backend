using Application.Features.HR.Commands.ApprovePayroll;
using Application.Features.HR.Queries.GetPayrollSummary;
using Application.Interfaces;
using Asp.Versioning;
using Domain.Entities.HR;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller for managing employee commission records.
/// </summary>
/// <param name="context">The database context.</param>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hr/commissions")]
[ApiController]
[Authorize]
public class CommissionController(IApplicationDBContext context) : ControllerBase
{
    /// <summary>
    /// Gets all commission records.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of commission records.</returns>
    [HttpGet]
    public async Task<ActionResult<List<CommissionRecord>>> GetRecords(CancellationToken cancellationToken)
    {
        return await context.CommissionRecords
            .Include(r => r.EmployeeProfile)
            .Include(r => r.Output)
            .OrderByDescending(r => r.DateEarned)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a summary of payroll (salary + commissions).
    /// </summary>
    [HttpGet("payroll-summary")]
    public async Task<IActionResult> GetPayrollSummary(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPayrollSummaryQuery(month, year), ct);
        return Ok(result.Value);
    }

    /// <summary>
    /// Approves commission payments.
    /// </summary>
    [HttpPost("approve-payroll")]
    public async Task<IActionResult> ApprovePayroll(
        [FromBody] ApprovePayrollCommand command,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok();
    }
}
