using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.DTOs.Chat;
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
        if (user == null) return NotFound("User not found");

        var session = await dbContext.ChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId, cancellationToken);

        if (session == null)
        {
            return NotFound("Session không tồn tại hoặc không thuộc quyền sở hữu.");
        }

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
            History = history,
            RoutingContext = session.RoutingContext
        });
    }

    [HttpPost("sessions/{sessionId}/routing-context")]
    public async Task<IActionResult> UpdateRoutingContext(Guid sessionId, [FromBody] UpdateRoutingContextRequest request, CancellationToken cancellationToken)
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

    // ---- Stage 10 — Plan Mode: DB thật cho plan_node/execute_step_node bên sidecar ----
    // Các endpoint này chỉ thuần mutate DB (giống UpdateRoutingContext ở trên) — sự kiện plan_*
    // do chính sidecar phát qua get_stream_writer() trong cùng lượt gọi, chảy qua stream JSON-lines
    // sẵn có tới ChatRunExecutor (nhánh catch-all) rồi ra SignalR, không append trùng ở đây.

    private const int MaxPlanSteps = 8;

    private async Task<ChatPlan?> GetOwnedPlanAsync(Guid runId, Guid userId, CancellationToken cancellationToken)
    {
        var plan = await dbContext.ChatPlans
            .Include(p => p.Run!.Session)
            .FirstOrDefaultAsync(p => p.RunId == runId, cancellationToken);
        return plan != null && plan.Run?.Session?.UserId == userId ? plan : null;
    }

    [HttpPost("runs/{runId}/plan/start")]
    public async Task<IActionResult> StartPlan(Guid runId, [FromBody] StartPlanRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var run = await dbContext.ChatRuns
            .Include(r => r.Session)
            .Include(r => r.Plan)
            .FirstOrDefaultAsync(r => r.Id == runId, cancellationToken);
        if (run == null || run.Session?.UserId != userId)
        {
            return NotFound("Run không tồn tại hoặc không thuộc quyền sở hữu.");
        }

        // Idempotent — resume/retry gọi lại start thì trả về plan đã có, không tạo trùng.
        if (run.Plan != null)
        {
            return Ok(new { planId = run.Plan.Id });
        }

        var plan = new ChatPlan
        {
            RunId = runId,
            SessionId = run.SessionId,
            Status = ChatPlanStatus.Drafting,
            Steps = "[]",
            ToolRegistryFingerprint = request.Fingerprint,
        };
        dbContext.ChatPlans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { planId = plan.Id });
    }

    [HttpGet("runs/{runId}/plan")]
    public async Task<IActionResult> GetPlanForSidecar(Guid runId, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var plan = await GetOwnedPlanAsync(runId, userId, cancellationToken);
        if (plan == null) return NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu.");

        var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps) ?? [];
        return Ok(new { planId = plan.Id, version = plan.Version, status = plan.Status, steps });
    }

    [HttpPost("runs/{runId}/plan/steps")]
    public async Task<IActionResult> AddPlanStep(Guid runId, [FromBody] AddPlanStepRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var plan = await GetOwnedPlanAsync(runId, userId, cancellationToken);
        if (plan == null) return NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu.");

        var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps) ?? [];
        var activeCount = steps.Count(s => s.Status != PlanStepStatus.Skipped);
        if (activeCount >= MaxPlanSteps)
        {
            return BadRequest("Kế hoạch đã đạt tối đa 8 bước.");
        }

        var step = PlanStepDto.NewPending(
            Guid.NewGuid().ToString("N"),
            steps.Count == 0 ? 1 : steps.Max(s => s.Order) + 1,
            request.Title,
            request.Detail,
            request.ExpectedTools ?? []);
        steps.Add(step);

        plan.Steps = JsonSerializer.Serialize(steps);
        plan.Version++;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(step);
    }

    [HttpPost("runs/{runId}/plan/ready")]
    public async Task<IActionResult> MarkPlanReady(Guid runId, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var plan = await GetOwnedPlanAsync(runId, userId, cancellationToken);
        if (plan == null) return NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu.");

        plan.Status = ChatPlanStatus.Ready;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPost("runs/{runId}/plan/steps/{stepId}/status")]
    public async Task<IActionResult> UpdatePlanStepStatus(
        Guid runId, string stepId, [FromBody] UpdatePlanStepStatusRequest request, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var plan = await GetOwnedPlanAsync(runId, userId, cancellationToken);
        if (plan == null) return NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu.");

        var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps) ?? [];
        var idx = steps.FindIndex(s => s.Id == stepId);
        if (idx < 0) return NotFound("Không tìm thấy bước.");

        steps[idx] = steps[idx] with { Status = request.Status, Result = request.Result ?? steps[idx].Result };
        plan.Steps = JsonSerializer.Serialize(steps);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
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
