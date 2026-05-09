using Application.Features.HR.Commands.CreateCommissionPolicy;
using Application.Features.HR.Commands.UpdateCommissionPolicy;
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
/// Controller for managing commission policies.
/// </summary>
/// <param name="context">The database context.</param>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hr/commission-policies")]
[ApiController]
[Authorize]
public class CommissionPolicyController : ControllerBase
{
    /// <summary>
    /// Gets all commission policies.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CommissionPolicy>>> GetPolicies(
        [FromServices] IApplicationDBContext context,
        CancellationToken cancellationToken)
    {
        return await context.CommissionPolicies
            .Include(p => p.Category)
            .Include(p => p.Product)
            .OrderByDescending(p => p.EffectiveDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new commission policy.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePolicy(
        [FromBody] CreateCommissionPolicyCommand command,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Updates an existing commission policy.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePolicy(
        int id,
        [FromBody] UpdateCommissionPolicyCommand command,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest();
        var result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return NoContent();
    }

    /// <summary>
    /// Gets audit logs for a policy.
    /// </summary>
    [HttpGet("{id}/audit-logs")]
    public async Task<ActionResult<List<CommissionPolicyAuditLog>>> GetAuditLogs(
        int id,
        [FromServices] IApplicationDBContext context,
        CancellationToken cancellationToken)
    {
        return await context.CommissionPolicyAuditLogs
            .Where(l => l.PolicyId == id)
            .OrderByDescending(l => l.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a commission policy.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePolicy(
        int id, 
        [FromServices] IApplicationDBContext context,
        CancellationToken cancellationToken)
    {
        var policy = await context.CommissionPolicies.FindAsync(id, cancellationToken);
        if (policy == null)
            return NotFound();
        context.CommissionPolicies.Remove(policy);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
