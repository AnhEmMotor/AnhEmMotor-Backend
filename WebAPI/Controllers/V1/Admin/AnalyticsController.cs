using Application.Features.Admin.Analytics;
using Infrastructure.DBContexts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ApplicationDBContext _db;

	public AnalyticsController(IMediator mediator, ApplicationDBContext db)
	{
		_mediator = mediator;
		_db = db;
	}

	[HttpGet("dashboard-kpis")]
	public async Task<IActionResult> GetKpis(CancellationToken ct = default)
	{
		var result = await _mediator.Send(new GetDashboardKpisQuery(), ct);
		return Ok(result);
	}

	[HttpGet("charts")]
	public async Task<IActionResult> GetCharts(CancellationToken ct = default)
	{
		var result = await _mediator.Send(new GetAnalyticsChartsQuery(), ct);
		return Ok(result);
	}

	[HttpGet("recent-audit-logs")]
	public async Task<IActionResult> GetRecentAuditLogs(
		[FromQuery] int limit = 20,
		[FromQuery] string[]? categories = null,
		[FromQuery] DateTimeOffset? fromDate = null,
		CancellationToken ct = default)
	{
		var cutoff = fromDate ?? DateTimeOffset.UtcNow.AddDays(-30);
		var logs = await _db.OrderStatusHistories
			.IgnoreQueryFilters()
			.Where(h => h.ChangedAt >= cutoff)
			.OrderByDescending(h => h.ChangedAt)
			.Take(limit)
			.Select(h => new
			{
				timestamp = h.ChangedAt,
				category = "order",
				action = "updated",
				actorId = (Guid?)h.ChangedBy,
				actorName = h.ChangedByUser != null ? h.ChangedByUser.FullName : "System",
				targetType = "Order",
				targetId = h.OutputId,
				targetName = string.Empty,
				details = h.Note ?? string.Empty
			})
			.ToListAsync(ct);
		return Ok(logs);
	}
}
