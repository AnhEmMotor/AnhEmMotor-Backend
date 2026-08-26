using Application.Features.Statistical.Queries.GetDashboardSummary;
using Application.Features.Statistical.Queries.GetPnlReport;
using Application.Features.Statistical.Queries.GetRecentTransactions;
using Application.Features.Statistical.Queries.GetStaffPerformance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for handling analytics and reporting data.
    /// </summary>
    [Authorize]
    [Route("api/analytics")]
    public class AnalyticsController(IMediator mediator) : ApiController
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
            var startDate = DateTime.SpecifyKind(start ?? DateTime.Today, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(end ?? DateTime.Today.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            var result = await mediator.Send(new GetDashboardSummaryQuery(startDate, endDate), cancellationToken);
            return HandleResult(result);
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
            var result = await mediator.Send(new GetPnlReportQuery(month, year), cancellationToken);
            return HandleResult(result);
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
            var startDate = NormalizeReportDate(start ?? DateTime.Today.AddDays(-30));
            var endDate = NormalizeReportDate(end ?? DateTime.Today);
            var result = await mediator.Send(new GetStaffPerformanceQuery(startDate, endDate), cancellationToken);
            return HandleResult(result);
        }

        internal static DateTime NormalizeReportDate(DateTime value)
        {
            return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
        }

        /// <summary>
        /// Gets the most recent transactions.
        /// </summary>
        /// <returns>A list of recent transactions.</returns>
        [HttpGet("transactions/recent")]
        public async Task<IActionResult> GetRecentTransactions(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetRecentTransactionsQuery(), cancellationToken);
            return HandleResult(result);
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
            var result = await mediator.Send(new GetRecentTransactionsQuery(), cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                foreach (var log in result.Value)
                {
                    var data = JsonSerializer.Serialize(log);
                    await Response.WriteAsync($"data: {data}\n\n", cancellationToken).ConfigureAwait(false);
                }
            }
            await Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
