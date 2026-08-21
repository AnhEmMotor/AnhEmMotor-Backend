using Application.Interfaces.Services.Marketing;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Google Ads Integration")]
[Route("api/v{version:apiVersion}/google-ads")]
[Authorize]
public class GoogleAdsController(IGoogleAdsService googleAdsService) : ApiController
{
    [HttpGet("metrics")]
    [SwaggerOperation(Summary = "Lấy dữ liệu metrics thực tế từ Google Ads")]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var data = await googleAdsService.GetCampaignPerformanceAsync(cancellationToken);
        return Ok(new { isSuccess = true, value = data });
    }
}
