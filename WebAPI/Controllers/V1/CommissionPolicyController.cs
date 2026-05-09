using Application.Features.HR.Commands.CreateCommissionPolicy;
using Application.Features.HR.Commands.DeleteCommissionPolicy;
using Application.Features.HR.Commands.UpdateCommissionPolicy;
using Application.Features.HR.Queries.GetCommissionPolicies;
using Application.Features.HR.Queries.GetCommissionPolicyAuditLogs;
using Asp.Versioning;
using Domain.Entities.HR;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller for managing commission policies.
/// </summary>
/// <param name="mediator">The mediator instance.</param>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hr/commission-policies")]
public class CommissionPolicyController(ISender mediator) : ApiController
{
    /// <summary>
    /// Gets all commission policies.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet]
    public async Task<ActionResult<List<CommissionPolicy>>> GetPoliciesAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommissionPoliciesQuery(), cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new commission policy.
    /// </summary>
    /// <param name="command">The create commission policy command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost]
    public async Task<IActionResult> CreatePolicyAsync(
        [FromBody] CreateCommissionPolicyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Updates an existing commission policy.
    /// </summary>
    /// <param name="id">The policy ID.</param>
    /// <param name="command">The update commission policy command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePolicyAsync(
        int id,
        [FromBody] UpdateCommissionPolicyCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return NoContent();
    }

    /// <summary>
    /// Gets audit logs for a policy.
    /// </summary>
    /// <param name="id">The policy ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("{id}/audit-logs")]
    public async Task<ActionResult<List<CommissionPolicyAuditLog>>> GetAuditLogsAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommissionPolicyAuditLogsQuery(id), cancellationToken)
            .ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a commission policy.
    /// </summary>
    /// <param name="id">The policy ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePolicyAsync(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCommissionPolicyCommand(id), cancellationToken).ConfigureAwait(true);
        if (!result.IsSuccess)
            return HandleResult(result);
        return NoContent();
    }
}
