using Application.Features.Marketing.Queries.GetVisitorTracking;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Marketing features")]
[Route("api/v{version:apiVersion}/marketing")]
[Authorize]
public class MarketingController(ISender sender) : ApiController
{
    [HttpGet("visitor-tracking")]
    [SwaggerOperation(Summary = "Lấy danh sách người dùng đã truy cập sản phẩm (Tracking)")]
    public async Task<IActionResult> GetVisitorTracking([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var query = new GetVisitorTrackingQuery(take);
        var result = await sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("product-views")]
    [SwaggerOperation(Summary = "Lấy lịch sử xem sản phẩm có phân trang (Marketing Dashboard)")]
    public async Task<IActionResult> GetProductViewHistory([FromQuery] Application.Features.Marketing.Queries.GetProductViewHistory.GetProductViewHistoryQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpGet("google-ads/campaigns")]
    [SwaggerOperation(Summary = "Lấy thống kê Google Ads")]
    public async Task<IActionResult> GetGoogleAdsCampaigns([FromServices] Application.Interfaces.Services.Marketing.IGoogleAdsService googleAdsService, CancellationToken cancellationToken)
    {
        var result = await googleAdsService.GetCampaignPerformanceAsync(cancellationToken);
        return Ok(result);
    }
}
