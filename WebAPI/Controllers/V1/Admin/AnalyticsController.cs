using Application.Features.Admin.Analytics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1.Admin
{
    [ApiController]
    [Route("api/v1/admin/analytics")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("dashboard-kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var result = await _mediator.Send(new GetDashboardKpisQuery());
            return Ok(result);
        }

        [HttpGet("charts")]
        public async Task<IActionResult> GetCharts()
        {
            var result = await _mediator.Send(new GetAnalyticsChartsQuery());
            return Ok(result);
        }
    }
}
