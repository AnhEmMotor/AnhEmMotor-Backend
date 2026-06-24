using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.ApiContracts.Admin.Analytics;
using Application.Features.Admin.Analytics;
using System.Threading.Tasks;

namespace AnhEmMotor.WebAPI.Controllers.V1.Admin
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
