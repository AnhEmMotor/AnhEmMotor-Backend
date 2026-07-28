using Application.Common.Models;
using Application.Features.Admin.Analytics;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using Infrastructure.DBContexts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Controllers.V1.Admin;

/// <summary>
/// API phân tích dữ liệu dành riêng cho vai trò Quản trị viên (Admin). Cung cấp các chỉ số KPI và dữ liệu biểu đồ tổng
/// hợp từ Dashboard.
/// </summary>
[ApiController]
[Route("api/v1/admin/analytics")]
[HasPermission(Permissions.Admin.DashboardManagement.View)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class AnalyticsController : ControllerBase
{
private readonly IMediator _mediator;
private readonly ApplicationDBContext _db;

/// <summary>
/// Khởi tạo <see cref="AnalyticsController" /> với MediatR mediator và DB context.
/// </summary>
/// <param name="mediator">MediatR mediator để gửi query xử lý phân tích.</param>
/// <param name="db">Database context.</param>
public AnalyticsController(IMediator mediator, ApplicationDBContext db)
{
_mediator = mediator;
_db = db;
}

/// <summary>
/// Lấy các chỉ số KPI tổng quan cho bảng điều khiển (Dashboard) của Quản trị viên.
/// </summary>
/// <param name="cancellationToken">Token hủy bỏ.</param>
/// <returns>Danh sách các chỉ số KPI (số đơn hàng, doanh thu, v.v.).</returns>
/// <response code="200">Trả về danh sách KPI thành công.</response>
[HttpGet("dashboard-kpis")]
[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
public async Task<IActionResult> GetKpis(CancellationToken cancellationToken = default)
{
var result = await _mediator.Send(new GetDashboardKpisQuery(), cancellationToken);
return Ok(result);
}

/// <summary>
/// Lấy dữ liệu biểu đồ phân tích tổng hợp cho Dashboard của Quản trị viên.
/// </summary>
/// <param name="cancellationToken">Token hủy bỏ.</param>
/// <returns>Dữ liệu các biểu đồ phân tích (doanh thu, đơn hàng, v.v.).</returns>
/// <response code="200">Trả về dữ liệu biểu đồ thành công.</response>
[HttpGet("charts")]
[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
public async Task<IActionResult> GetCharts(CancellationToken cancellationToken = default)
{
var result = await _mediator.Send(new GetAnalyticsChartsQuery(), cancellationToken);
return Ok(result);
}

/// <summary>
/// Lấy danh sách hoạt động gần đây trên hệ thống (audit log tổng hợp).
/// Gom dữ liệu từ OrderStatusHistories, đơn hàng mới và chi phí.
/// </summary>
/// <param name="limit">Số bản ghi tối đa trả về.</param>
/// <param name="categories">Lọc theo danh mục (order, order_created, finance).</param>
/// <param name="fromDate">Lấy các bản ghi từ thời điểm này.</param>
/// <param name="ct">Token hủy bỏ.</param>
/// <returns>Danh sách audit log gần đây.</returns>
/// <response code="200">Trả về danh sách audit log thành công.</response>
[HttpGet("recent-audit-logs")]
[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
public async Task<IActionResult> GetRecentAuditLogs(
[FromQuery] int limit = 20,
[FromQuery] string[]? categories = null,
[FromQuery] DateTimeOffset? fromDate = null,
CancellationToken ct = default)
{
var cutoff = fromDate ?? DateTimeOffset.UtcNow.AddDays(-30);
var categorySet = categories != null && categories.Length > 0
? new HashSet<string>(categories, StringComparer.OrdinalIgnoreCase)
: null;

var logs = new List<object>();

if (categorySet == null || categorySet.Contains("order"))
{
var orderLogs = await _db.OrderStatusHistories
.IgnoreQueryFilters()
.Where(h => h.ChangedAt >= cutoff)
.OrderByDescending(h => h.ChangedAt)
.Take(limit)
.Select(
h => new
{
timestamp = (DateTimeOffset?)h.ChangedAt,
category = (string)"order",
action = (string)"updated",
actorId = (Guid?)h.ChangedBy,
actorName = h.ChangedByUser != null ? h.ChangedByUser.FullName : "System",
targetType = (string)"Order",
targetId = (int?)h.OutputId,
targetName = (string?)string.Empty,
details = (string?)(h.Note ?? string.Empty)
})
.ToListAsync(ct);
logs.AddRange(orderLogs);
}

if (categorySet == null || categorySet.Contains("order_created"))
{
var newOrders = await _db.OutputOrders
.IgnoreQueryFilters()
.Where(o => o.CreatedAt >= cutoff)
.OrderByDescending(o => o.CreatedAt)
.Take(limit)
.Select(
o => new
{
timestamp = (DateTimeOffset?)o.CreatedAt,
category = (string)"order",
action = (string)"created",
actorId = (Guid?)o.CreatedBy,
actorName = o.CreatedByUser != null ? o.CreatedByUser.FullName : "System",
targetType = (string)"Order",
targetId = (int?)o.Id,
targetName = (string?)(o.CustomerName ?? string.Empty),
details = (string?)($"Don hang moi - {o.CustomerName ?? "N/A"}")
})
.ToListAsync(ct);
logs.AddRange(newOrders);
}

if (categorySet == null || categorySet.Contains("finance"))
{
var expenseLogs = await _db.Expenses
.OrderByDescending(e => e.ExpenseDate)
.Take(limit)
.Select(
e => new
{
timestamp = (DateTimeOffset?)e.ExpenseDate,
category = (string)"finance",
action = (string)"expense",
actorId = (Guid?)null,
actorName = (string)"System",
targetType = (string)"Expense",
targetId = (int?)e.Id,
targetName = (string?)(e.Name ?? string.Empty),
details = (string?)($"Chi phi: {e.Name} - {e.Amount:N0} VND")
})
.ToListAsync(ct)
.ConfigureAwait(false);
logs.AddRange(expenseLogs);
}

var result = logs
.OrderByDescending(l => ((dynamic)l).timestamp)
.Take(limit)
.ToList();

return Ok(result);
}
}
