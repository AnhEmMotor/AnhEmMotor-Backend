using Application.Common.Models;
using Application.Features.Admin.Analytics;
using Infrastructure.DBContexts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers.V1.Admin;

/// <summary>
/// API phân tích dữ liệu dành riêng cho vai trò Quản trị viên (Admin). Cung cấp các chỉ số KPI và dữ liệu biểu đồ tổng
/// hợp từ Dashboard.
/// </summary>
[ApiController]
[Route("api/v1/admin/analytics")]
[Authorize(Roles = "Admin")]
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
