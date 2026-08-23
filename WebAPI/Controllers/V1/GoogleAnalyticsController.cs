using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Ga4;
using Application.Interfaces.Services.Analytics;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using Infrastructure.Configurations.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Cổng Google Analytics 4 duy nhất của hệ thống: trả Measurement ID cho frontend/mobile (key không bao giờ
/// nằm ở client), nhận sự kiện từ Mobile để forward sang Measurement Protocol, và phục vụ chỉ số cho
/// dashboard Management + AI chat tool.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Google Analytics 4")]
[Route("api/v{version:apiVersion}/analytics")]
public class GoogleAnalyticsController(
    IOptions<GoogleAnalytics4Options> gaOptions,
    IGa4AnalyticsService ga4AnalyticsService,
    IGa4MeasurementProtocolService measurementProtocolService,
    IServerDateProvider dateProvider) : ApiController
{
    /// <summary>Cấu hình tracking công khai cho các app client — chỉ chứa Measurement ID của Store, không có secret nào.</summary>
    [HttpGet("public-config")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Lấy cấu hình GA4 công khai (Measurement ID) cho Store/Mobile")]
    public IActionResult GetPublicConfig()
    {
        var options = gaOptions.Value;
        return Ok(
            new Ga4PublicConfigDto(
                !string.IsNullOrWhiteSpace(options.MeasurementId),
                options.MeasurementId ?? string.Empty));
    }

    /// <summary>Mobile/app client POST sự kiện thô vào đây; Backend tự gắn secret rồi forward sang GA4.</summary>
    [HttpPost("events")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Nhận sự kiện tracking từ Mobile và forward lên GA4 Measurement Protocol")]
    public async Task<IActionResult> TrackEvents(
        [FromBody] Ga4TrackEventsRequest request,
        CancellationToken cancellationToken)
    {
        if (!measurementProtocolService.IsConfigured())
        {
            return NoContent();
        }

        if (string.IsNullOrWhiteSpace(request.ClientId) || request.ClientId.Length > 128)
        {
            return BadRequest(ErrorResponse.FromError(Error.Failure("clientId không hợp lệ.")));
        }

        if (request.Events is null || request.Events.Count == 0)
        {
            return NoContent();
        }

        var result = await measurementProtocolService.SendEventsAsync(
                request.ClientId.Trim(),
                request.UserId,
                request.Events
                    .Take(10)
                    .Select(e => new MeasurementProtocolEvent(
                        e.Name,
                        e.Timestamp ?? dateProvider.VietnamNow,
                        e.Params))
                    .ToArray(),
                cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>Tổng quan chỉ số truy cập trong khoảng thời gian.</summary>
    [HttpGet("overview")]
    [Authorize]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    [SwaggerOperation(Summary = "Tổng quan chỉ số GA4 (sessions, users, pageviews...) theo khoảng ngày")]
    public async Task<IActionResult> GetOverview(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        if (!ga4AnalyticsService.IsConfigured())
        {
            return Ok(new Ga4OverviewDto());
        }

        var (start, end) = ResolveRange(from, to);
        var result = await ga4AnalyticsService.GetOverviewAsync(start, end, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>Số liệu theo ngày phục vụ vẽ biểu đồ.</summary>
    [HttpGet("daily")]
    [Authorize]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    [SwaggerOperation(Summary = "Chuỗi số liệu GA4 theo ngày")]
    public async Task<IActionResult> GetDailySeries(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        if (!ga4AnalyticsService.IsConfigured())
        {
            return Ok(new Ga4ReportDto<Ga4DimensionRowDto>());
        }

        var (start, end) = ResolveRange(from, to);
        var result = await ga4AnalyticsService.GetDailySeriesAsync(start, end, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>Top nguồn traffic đưa người dùng vào web/app.</summary>
    [HttpGet("sources")]
    [Authorize]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    [SwaggerOperation(Summary = "Top nguồn traffic GA4 (sessionSource)")]
    public async Task<IActionResult> GetTopSources(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (!ga4AnalyticsService.IsConfigured())
        {
            return Ok(new Ga4ReportDto<Ga4DimensionRowDto>());
        }

        var (start, end) = ResolveRange(from, to);
        var result = await ga4AnalyticsService.GetTopSourcesAsync(start, end, Math.Clamp(limit, 1, 25), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>Top trang được xem nhiều nhất.</summary>
    [HttpGet("pages")]
    [Authorize]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    [SwaggerOperation(Summary = "Top trang GA4 (pagePath) theo lượt xem")]
    public async Task<IActionResult> GetTopPages(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (!ga4AnalyticsService.IsConfigured())
        {
            return Ok(new Ga4ReportDto<Ga4DimensionRowDto>());
        }

        var (start, end) = ResolveRange(from, to);
        var result = await ga4AnalyticsService.GetTopPagesAsync(start, end, Math.Clamp(limit, 1, 25), cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>Phân rã truy cập theo loại thiết bị.</summary>
    [HttpGet("devices")]
    [Authorize]
    [HasPermission(Permissions.Admin.DashboardManagement.View)]
    [SwaggerOperation(Summary = "Phân rã truy cập GA4 theo thiết bị")]
    public async Task<IActionResult> GetDeviceBreakdown(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        if (!ga4AnalyticsService.IsConfigured())
        {
            return Ok(new Ga4ReportDto<Ga4DimensionRowDto>());
        }

        var (start, end) = ResolveRange(from, to);
        var result = await ga4AnalyticsService.GetDeviceBreakdownAsync(start, end, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    private (DateOnly Start, DateOnly End) ResolveRange(DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(dateProvider.VietnamNow.DateTime);
        var end = to.HasValue && to.Value <= today ? to.Value : today;
        var start = from.HasValue && from.Value < end ? from.Value : end.AddDays(-29);
        return (start, end);
    }
}
