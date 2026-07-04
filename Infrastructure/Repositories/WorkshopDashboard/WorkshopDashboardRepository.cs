using Application.Features.Statistical.DTOs;
using Application.Interfaces.Repositories.WorkshopDashboard;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SqlServerDb = Microsoft.EntityFrameworkCore.SqlServerDbFunctionsExtensions;

namespace Infrastructure.Repositories.WorkshopDashboard;

public class WorkshopDashboardRepository : IWorkshopDashboardRepository
{
    private readonly ApplicationDBContext _context;

    public WorkshopDashboardRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<WorkshopDashboardDto> GetOverviewAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken)
    {
        var result = new WorkshopDashboardDto();

        result.SummaryCards = new SummaryCardsDto
        {
            TotalBookings = await _context.Bookings
                .CountAsync(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate, cancellationToken),
            TotalRepairOrders = await _context.RepairOrders
                .CountAsync(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate, cancellationToken),
            TotalMaintenances = await _context.MaintenanceHistories
                .CountAsync(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate, cancellationToken),
            TotalWarrantyClaims = await _context.WarrantyClaims
                .CountAsync(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate, cancellationToken),
            AvgCompletionHours = await _context.RepairOrders
                .Where(r => r.CompletedDate.HasValue
                         && r.StartTime.HasValue
                         && r.CreatedAt >= startDate
                         && r.CreatedAt <= endDate)
                .Select(r => (double?)(
                    SqlServerDb.DateDiffSecond(EF.Functions, r.StartTime, r.CompletedDate) / 3600.0))
                .AverageAsync(cancellationToken) ?? 0.0
        };

        var payments = _context.WorkshopPayments
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

        result.FinancialSummary = new FinancialSummaryDto
        {
            TotalRevenue = await payments
                .Where(p => p.PaymentStatus == "Paid")
                .SumAsync(p => (decimal?)p.TotalAmount, cancellationToken) ?? 0,
            TotalUnpaidAmount = await payments
                .Where(p => p.PaymentStatus == "Unpaid")
                .SumAsync(p => (decimal?)p.TotalAmount, cancellationToken) ?? 0,
            TotalPartialAmount = await payments
                .Where(p => p.PaymentStatus == "Partial")
                .SumAsync(p => (decimal?)p.TotalAmount, cancellationToken) ?? 0,
            UnpaidInvoicesCount = await payments
                .CountAsync(p => p.PaymentStatus == "Unpaid", cancellationToken)
        };

        result.DailyRevenues = await _context.WorkshopPayments
            .Where(p => p.PaymentStatus == "Paid"
                     && p.PaidAt >= startDate
                     && p.PaidAt <= endDate)
            .GroupBy(p => p.PaidAt!.Value.Date)
            .Select(g => new DailyRevenueDto
            {
                RevenueDate = g.Key,
                DailyRevenue = g.Sum(p => p.TotalAmount)
            })
            .OrderBy(d => d.RevenueDate)
            .ToListAsync(cancellationToken);

        result.TopServices = await _context.RepairOrderDetails
            .Where(rod => rod.Type == "Service"
                       && rod.Service != null
                       && rod.RepairOrder != null
                       && rod.RepairOrder.CreatedAt >= startDate
                       && rod.RepairOrder.CreatedAt <= endDate)
            .GroupBy(rod => new { rod.ServiceId, rod.Service!.Name })
            .Select(g => new TopServiceDto
            {
                ServiceName = g.Key.Name,
                UsageCount = g.Count(),
                TotalServiceRevenue = g.Sum(rod => rod.Price * rod.Count)
            })
            .OrderByDescending(t => t.UsageCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        result.StatusBreakdowns = await _context.RepairOrders
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .GroupBy(r => r.Status)
            .Select(g => new StatusBreakdownDto
            {
                Status = g.Key,
                StatusCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        return result;
    }
}
