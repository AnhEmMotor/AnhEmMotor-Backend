using Application.DTOs.Chat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebAPI.Attributes;

namespace WebAPI.Controllers;

[Route("internal/chat")]
[ApiController]
[Authorize]
[LocalhostOnly]
public class InternalChatController(
    UserManager<ApplicationUser> userManager,
    ApplicationDBContext dbContext,
    IConfiguration configuration,
    IChatRunWriter chatRunWriter) : ControllerBase
{
    [HttpPost("context")]
    public async Task<IActionResult> GetContext([FromBody] ContextRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }
        var user = await userManager.FindByIdAsync(userIdString);
        if (user == null)
            return NotFound("User not found");
        var session = await dbContext.ChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId, cancellationToken);
        if (session == null)
        {
            return NotFound("Session không tồn tại hoặc không thuộc quyền sở hữu.");
        }
        var roles = await userManager.GetRolesAsync(user);
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ??
            new List<string>();
        List<string> rolePermissions;
        if (roles.Any(r => superRoles.Contains(r)))
        {
            rolePermissions = await dbContext.Permissions
                .Where(p => p.Name != null)
                .Select(p => p.Name!)
                .ToListAsync(cancellationToken);
        } else
        {
            rolePermissions = await dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Include(rp => rp.Role)
                .Where(rp => rp.Role != null && roles.Contains(rp.Role.Name!))
                .Select(rp => rp.Permission!.Name!)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
        var limit = Math.Clamp(request.HistoryLimit, 1, 50);
        var history = await dbContext.ChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == request.SessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new { m.Role, m.Message, m.CreatedAt })
            .ToListAsync(cancellationToken);
        return Ok(
            new
            {
                User = new { user.Id, user.UserName, user.FullName, user.Email },
                Roles = roles,
                Permissions = rolePermissions,
                request.SessionId,
                History = history,
                session.RoutingContext
            });
    }

    [HttpPost("sessions/{sessionId}/routing-context")]
    public async Task<IActionResult> UpdateRoutingContext(
        Guid sessionId,
        [FromBody] UpdateRoutingContextRequest request,
        CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }
        var session = await dbContext.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);
        if (session == null)
        {
            return NotFound("Session không tồn tại hoặc không thuộc quyền sở hữu.");
        }
        session.RoutingContext = request.RoutingContext;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPost("runs/{runId}/pull-steering")]
    public async Task<IActionResult> PullSteering(Guid runId, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }
        var run = await dbContext.ChatRuns
            .AsNoTracking()
            .Include(r => r.Session)
            .FirstOrDefaultAsync(r => r.Id == runId, cancellationToken);
        if (run == null || run.Session?.UserId != userId)
        {
            return NotFound("Run không tồn tại hoặc không thuộc quyền sở hữu.");
        }
        var items = await chatRunWriter.PullPendingSteeringAsync(runId);
        return Ok(items);
    }

}

public class ContextRequest
{
    public Guid SessionId { get; set; }

    public string Message { get; set; } = string.Empty;

    public int HistoryLimit { get; set; } = 20;
}

public class UpdateRoutingContextRequest
{
    public string RoutingContext { get; set; } = "{}";
}

public class StartPlanRequest
{
    public string? Fingerprint { get; set; }
}

public class AddPlanStepRequest
{
    public string Title { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public List<string>? ExpectedTools { get; set; }
}

public class UpdatePlanStepStatusRequest
{
    public string Status { get; set; } = string.Empty;

    public string? Result { get; set; }
}

public class CreatePlanTemplateRequest
{
    public string CanonicalQuestion { get; set; } = string.Empty;

    public string IntentHash { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public string StepsTemplateJson { get; set; } = "[]";

    public string SlotsJson { get; set; } = "[]";

    public string RequiredToolsJson { get; set; } = "[]";

    public string RequiredPermissionsJson { get; set; } = "[]";

    public string? ToolRegistryFingerprint { get; set; }
}

public class RecordPlanTemplateUseRequest
{
    public bool Success { get; set; }

    public bool UserEdited { get; set; }

    public bool Rejected { get; set; }
}

