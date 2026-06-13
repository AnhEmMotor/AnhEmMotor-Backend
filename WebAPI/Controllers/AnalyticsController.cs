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
    /// <remarks>
    /// Initializes a new instance of the <see cref="AnalyticsController" /> class.
    /// </remarks>
    /// <param name="mediator">The mediator service.</param>
    [Authorize]
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Gets the dashboard summary for a specified date range.
        /// </summary>
        /// <param name="start">The start date. Defaults to today.</param>
        /// <param name="end">The end date. Defaults to end of today.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The dashboard summary.</returns>
        [HttpGet("dashboard/summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            CancellationToken cancellationToken)
        {
            var startDate = start ?? DateTime.Today;
            var endDate = end ?? DateTime.Today.AddDays(1).AddTicks(-1);
            var summary = await mediator.Send(new GetDashboardSummaryQuery(startDate, endDate), cancellationToken)
                .ConfigureAwait(false);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The PnL report.</returns>
        [HttpGet("pnl")]
        public async Task<IActionResult> GetPnl(
            [FromQuery] int month,
            [FromQuery] int year,
            CancellationToken cancellationToken)
        {
            var report = await mediator.Send(new GetPnlReportQuery(month, year), cancellationToken)
                .ConfigureAwait(false);
            return Ok(report);
        }

        /// <summary>
        /// Gets staff performance metrics for a specified date range.
        /// </summary>
        /// <param name="start">The start date. Defaults to 30 days ago.</param>
        /// <param name="end">The end date. Defaults to today.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The staff performance data.</returns>
        [HttpGet("staff-performance")]
        public async Task<IActionResult> GetStaff(
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            CancellationToken cancellationToken)
        {
            var startDate = start ?? DateTime.Today.AddDays(-30);
            var endDate = end ?? DateTime.Today;
            var performance = await mediator.Send(new GetStaffPerformanceQuery(startDate, endDate), cancellationToken)
                .ConfigureAwait(false);
            return Ok(performance);
        }

        /// <summary>
        /// Gets the most recent transactions.
        /// </summary>
        /// <returns>A list of recent transactions.</returns>
        [HttpGet("transactions/recent")]
        public async Task<IActionResult> GetRecentTransactions(CancellationToken cancellationToken)
        {
            var logs = await mediator.Send(new GetRecentTransactionsQuery(), cancellationToken).ConfigureAwait(false);
            return Ok(logs);
        }

        /// <summary>
        /// Streams transaction logs in real-time using Server-Sent Events (SSE).
        /// </summary>
        [HttpGet("stream/transactions")]
        public async Task GetTransactionStream(CancellationToken cancellationToken)
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
