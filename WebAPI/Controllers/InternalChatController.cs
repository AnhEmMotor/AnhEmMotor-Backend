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
    IChatRunWriter chatRunWriter,
    IChatReadRepository chatReadRepository,
    IChatInsertRepository chatInsertRepository,
    IChatUpdateRepository chatUpdateRepository,
    IUnitOfWork unitOfWork) : ControllerBase
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
                SessionId = request.SessionId,
                History = history,
                RoutingContext = session.RoutingContext
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

    private const int MaxPlanSteps = 8;

    private async Task<ChatPlan?> GetOwnedPlanAsync(Guid runId, Guid userId, CancellationToken cancellationToken)
    {
        var plan = await dbContext.ChatPlans
            .Include(p => p.Run!.Session)
            .FirstOrDefaultAsync(p => p.RunId == runId, cancellationToken);
        return plan != null && plan.Run?.Session?.UserId == userId ? plan : null;
    }

    [HttpPost("runs/{runId}/plan/start")]
    public async Task<IActionResult> StartPlan(
        Guid runId,
        [FromBody] StartPlanRequest request,
        CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();
        var run = await dbContext.ChatRuns
            .Include(r => r.Session)
            .Include(r => r.Plan)
            .FirstOrDefaultAsync(r => r.Id == runId, cancellationToken);
        if (run == null || run.Session?.UserId != userId)
        {
            return NotFound("Run không tồn tại hoặc không thuộc quyền sở hữu.");
        }
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
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();
        var plan = await GetOwnedPlanAsync(runId, userId, cancellationToken);
        if (plan == null)
            return NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu.");
        var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps) ?? [];
        return Ok(new { planId = plan.Id, version = plan.Version, status = plan.Status, steps });
    }

    [HttpPost("runs/{runId}/plan/steps")]
    public async Task<IActionResult> AddPlanStep(
        Guid runId,
        [FromBody] AddPlanStepRequest request,
        CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();
        var plan = await GetOwnedPlanAsync(runId, userId, cancellationToken);
        if (plan == null)
            return NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu.");
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
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();
        var plan = await GetOwnedPlanAsync(runId, userId, cancellationToken);
        if (plan == null)
            return NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu.");
        plan.Status = ChatPlanStatus.Ready;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPost("runs/{runId}/plan/steps/{stepId}/status")]
    public async Task<IActionResult> UpdatePlanStepStatus(
        Guid runId,
        string stepId,
        [FromBody] UpdatePlanStepStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
            return Unauthorized();
        var plan = await GetOwnedPlanAsync(runId, userId, cancellationToken);
        if (plan == null)
            return NotFound("Plan không tồn tại hoặc không thuộc quyền sở hữu.");
        var steps = JsonSerializer.Deserialize<List<PlanStepDto>>(plan.Steps) ?? [];
        var idx = steps.FindIndex(s => s.Id == stepId);
        if (idx < 0)
            return NotFound("Không tìm thấy bước.");
        steps[idx] = steps[idx] with { Status = request.Status, Result = request.Result ?? steps[idx].Result };
        plan.Steps = JsonSerializer.Serialize(steps);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpGet("plan-templates/find")]
    public async Task<IActionResult> FindPlanTemplate(
        [FromQuery] string intentHash,
        [FromQuery] string module,
        CancellationToken cancellationToken)
    {
        var template = await chatReadRepository.GetActiveTemplateByIntentHashAsync(
            intentHash,
            module,
            cancellationToken);
        if (template == null)
            return NotFound();
        return Ok(ToPlanTemplateResponse(template));
    }

    [HttpGet("plan-templates/{id:guid}")]
    public async Task<IActionResult> GetPlanTemplate(Guid id, CancellationToken cancellationToken)
    {
        var template = await chatReadRepository.GetTemplateByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound();
        return Ok(ToPlanTemplateResponse(template));
    }

    [HttpPost("plan-templates")]
    public async Task<IActionResult> CreatePlanTemplate(
        [FromBody] CreatePlanTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var template = new ChatPlanTemplate
        {
            CanonicalQuestion = request.CanonicalQuestion,
            IntentHash = request.IntentHash,
            Module = request.Module,
            StepsTemplate = request.StepsTemplateJson,
            Slots = request.SlotsJson,
            RequiredTools = request.RequiredToolsJson,
            RequiredPermissions = request.RequiredPermissionsJson,
            ToolRegistryFingerprint = request.ToolRegistryFingerprint,
        };
        chatInsertRepository.AddTemplate(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { templateId = template.Id });
    }

    [HttpPost("plan-templates/{id:guid}/record-use")]
    public async Task<IActionResult> RecordPlanTemplateUse(
        Guid id,
        [FromBody] RecordPlanTemplateUseRequest request,
        CancellationToken cancellationToken)
    {
        var template = await chatReadRepository.GetTemplateByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound();
        template.UseCount++;
        if (request.Success)
            template.SuccessCount++;
        if (request.UserEdited)
            template.UserEditCount++;
        if (request.Rejected)
            template.RejectCount++;
        template.LastUsedAt = DateTimeOffset.UtcNow;
        chatUpdateRepository.UpdateTemplate(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    private static object ToPlanTemplateResponse(ChatPlanTemplate template) => new
    {
        templateId = template.Id,
        canonicalQuestion = template.CanonicalQuestion,
        module = template.Module,
        stepsTemplate = JsonSerializer.Deserialize<JsonElement>(template.StepsTemplate),
        slots = JsonSerializer.Deserialize<JsonElement>(template.Slots),
        requiredTools = JsonSerializer.Deserialize<List<string>>(template.RequiredTools) ?? [],
        requiredPermissions = JsonSerializer.Deserialize<List<string>>(template.RequiredPermissions) ?? [],
        toolRegistryFingerprint = template.ToolRegistryFingerprint,
        useCount = template.UseCount,
        successCount = template.SuccessCount,
        userEditCount = template.UserEditCount,
        status = template.Status,
    };
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
