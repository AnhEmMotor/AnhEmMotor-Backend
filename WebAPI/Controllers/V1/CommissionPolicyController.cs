using Application.Common.Models;
using Application.Interfaces;
using Asp.Versioning;
using Domain.Entities.HR;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller for managing commission policies.
/// </summary>
/// <param name="context">The database context.</param>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hr/commission-policies")]
[ApiController]
[Authorize]
public class CommissionPolicyController(IApplicationDBContext context) : ControllerBase
{
    /// <summary>
    /// Gets all commission policies.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of commission policies.</returns>
    [HttpGet]
    public async Task<ActionResult<List<CommissionPolicy>>> GetPolicies(CancellationToken cancellationToken)
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
    public async Task<IActionResult> CreatePolicy([FromBody] Application.Features.HR.Commands.CreateCommissionPolicy.CreateCommissionPolicyCommand command, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Updates an existing commission policy.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePolicy(int id, [FromBody] Application.Features.HR.Commands.UpdateCommissionPolicy.UpdateCommissionPolicyCommand command, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return NoContent();
    }

    /// <summary>
    /// Gets audit logs for a policy.
    /// </summary>
    [HttpGet("{id}/audit-logs")]
    public async Task<ActionResult<List<CommissionPolicyAuditLog>>> GetAuditLogs(int id, CancellationToken cancellationToken)
    {
        return await context.CommissionPolicyAuditLogs
            .Where(l => l.PolicyId == id)
            .OrderByDescending(l => l.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a commission policy.
    /// </summary>
    /// <param name="id">The ID of the policy to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A no content result.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePolicy(int id, CancellationToken cancellationToken)
    {
        var policy = await context.CommissionPolicies.FindAsync(id, cancellationToken);
        if (policy == null) return NotFound();

        context.CommissionPolicies.Remove(policy);
        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
