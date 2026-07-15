using Application.Common.Models;
using Application.Features.Admin.Analytics;
using Domain.Primitives;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebAPI.Controllers.V1.Admin;

/// <summary>
/// API phân tích dữ liệu dành riêng cho vai trò Quản trị viên (Admin).
/// Cung cấp các chỉ số KPI và dữ liệu biểu đồ tổng hợp từ Dashboard.
/// </summary>
[ApiController]
[Route("api/v1/admin/analytics")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Khởi tạo <see cref="AnalyticsController"/> với MediatR mediator.
    /// </summary>
    /// <param name="mediator">MediatR mediator để gửi query xử lý phân tích.</param>
    public AnalyticsController(IMediator mediator) => _mediator = mediator;

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
}
