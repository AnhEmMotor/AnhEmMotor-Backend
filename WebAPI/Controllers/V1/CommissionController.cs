using Application.ApiContracts.HR.Responses;
using Application.Features.HR.Commands.ApprovePayroll;
using Application.Features.HR.Queries.GetCommissionRecords;
using Application.Features.HR.Queries.GetPayrollSummary;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Entities.HR;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller for managing employee commission records.
/// </summary>
/// <param name="mediator">The mediator instance.</param>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hr/commissions")]
public class CommissionController(ISender mediator) : ApiController
{
    /// <summary>
    /// Gets all commission records.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of commission records.</returns>
    [HttpGet]
    public async Task<ActionResult<List<CommissionRecord>>> GetRecordsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommissionRecordsQuery(), cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Gets a summary of payroll (salary + commissions).
    /// </summary>
    /// <param name="month">The month.</param>
    /// <param name="year">The year.</param>
    /// <param name="ct">The cancellation token.</param>
    [HttpGet("payroll-summary")]
    [HasPermission(PermissionsList.Payroll.View)]
    [ProducesResponseType(typeof(List<PayrollResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollSummaryAsync(
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPayrollSummaryQuery(month, year), ct).ConfigureAwait(true);
        return Ok(result.Value);
    }

    /// <summary>
    /// Approves commission payments.
    /// </summary>
    /// <param name="command">The approve payroll command.</param>
    /// <param name="ct">The cancellation token.</param>
    [HttpPost("approve-payroll")]
    [HasPermission(PermissionsList.Payroll.Approve)]
    public async Task<IActionResult> ApprovePayrollAsync([FromBody] ApprovePayrollCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct).ConfigureAwait(true);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok();
    }
}
