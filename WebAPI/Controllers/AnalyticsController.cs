using Application.Features.Statistical.Queries.GetDashboardSummary;
using Application.Features.Statistical.Queries.GetPnlReport;
using Application.Features.Statistical.Queries.GetRecentTransactions;
using Application.Features.Statistical.Queries.GetStaffPerformance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for handling analytics and reporting data.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsController"/> class.
        /// </summary>
        /// <param name="analyticsRepository">The analytics repository.</param>
        public AnalyticsController(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        /// <summary>
        /// Gets the dashboard summary for a specified date range.
        /// </summary>
        /// <param name="start">The start date. Defaults to today.</param>
        /// <param name="end">The end date. Defaults to end of today.</param>
        /// <returns>The dashboard summary.</returns>
        [HttpGet("dashboard/summary")]
        public async Task<IActionResult> GetSummary([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var startDate = start ?? DateTime.Today;
            var endDate = end ?? DateTime.Today.AddDays(1).AddTicks(-1);

            var summary = await _analyticsRepository.GetDashboardSummaryAsync(startDate, endDate);

            // Logic rào chắn màu sắc cho UI
            var now = DateTime.Now;
            if (now.Hour >= 15 && summary.MonthAchieved < (summary.MonthTarget * 0.5m))
            {
                summary.IsRevenueAlert = true;
            }
            return Ok(summary);
        }

        /// <summary>
        /// Gets the Profit and Loss (PnL) report for a specified month and year.
        /// </summary>
        /// <param name="month">The month (1-12).</param>
        /// <param name="year">The year.</param>
        /// <returns>The PnL report.</returns>
        [HttpGet("pnl")]
        public async Task<IActionResult> GetPnl([FromQuery] int month, [FromQuery] int year)
        {
            var report = await _analyticsRepository.GetPnlReportAsync(month, year);
            return Ok(report);
        }

        /// <summary>
        /// Gets staff performance metrics for a specified date range.
        /// </summary>
        /// <param name="start">The start date. Defaults to 30 days ago.</param>
        /// <param name="end">The end date. Defaults to today.</param>
        /// <returns>The staff performance data.</returns>
        [HttpGet("staff-performance")]
        public async Task<IActionResult> GetStaff([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var startDate = start ?? DateTime.Today.AddDays(-30);
            var endDate = end ?? DateTime.Today;

            var performance = await _analyticsRepository.GetStaffPerformanceAsync(startDate, endDate);
            return Ok(performance);
        }

        /// <summary>
        /// Gets the most recent transactions.
        /// </summary>
        /// <returns>A list of recent transactions.</returns>
        [HttpGet("transactions/recent")]
        public async Task<IActionResult> GetRecentTransactions()
        {
            var logs = await _analyticsRepository.GetRecentTransactionsAsync();
            return Ok(logs);
        }

        /// <summary>
        /// Streams transaction logs in real-time using Server-Sent Events (SSE).
        /// </summary>
        [HttpGet("stream/transactions")]
        public async Task GetTransactionStream()
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");
            var logs = await mediator.Send(new GetRecentTransactionsQuery(), cancellationToken).ConfigureAwait(false);
            foreach (var log in logs)
            {
                var data = JsonSerializer.Serialize(log);
                await Response.WriteAsync($"data: {data}\n\n", cancellationToken).ConfigureAwait(false);
            }
            await Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
