using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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
    IChatRunWriter chatRunWriter,
    ILogger<InternalChatController> logger) : ControllerBase
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
        if (user == null) return NotFound("User not found");

        var session = await dbContext.ChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId, cancellationToken);

        if (session == null)
            return NotFound("Session không tồn tại hoặc không thuộc quyền sở hữu.");

        var roles = await userManager.GetRolesAsync(user);

        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? new List<string>();

        List<string> rolePermissions;
        if (roles.Any(r => superRoles.Contains(r)))
        {
            rolePermissions = await dbContext.Permissions
                .Where(p => p.Name != null)
                .Select(p => p.Name!)
                .ToListAsync(cancellationToken);
        }
        else
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

        return Ok(new
        {
            User = new
            {
                user.Id,
                user.UserName,
                user.FullName,
                user.Email
            },
            Roles = roles,
            Permissions = rolePermissions,
            SessionId = request.SessionId,
            History = history
        });
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
