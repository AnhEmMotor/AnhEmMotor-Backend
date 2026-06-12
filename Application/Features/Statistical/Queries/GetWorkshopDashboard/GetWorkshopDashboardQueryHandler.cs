using Application.Api.Contracts.Statistical.Responses;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Statistical.Queries.GetWorkshopDashboard;

public class GetWorkshopDashboardQueryHandler : IRequestHandler<GetWorkshopDashboardQuery, WorkshopDashboardResponse>
{
    private readonly IApplicationDbContext _context;

    public GetWorkshopDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkshopDashboardResponse> Handle(GetWorkshopDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var response = new WorkshopDashboardResponse();

        // 1. KPI Cards
        var inProgressOrders = await _context.RepairOrders
            .Where(ro => ro.Status == "InProgress")
            .ToListAsync(cancellationToken);

        var completedOrders = await _context.RepairOrders
            .Where(ro => ro.Status == "Completed" && ro.StartTime != null && ro.CompletedDate != null)
            .ToListAsync(cancellationToken);

        var revenueOrders = await _context.RepairOrders
            .Where(ro => ro.CreatedAt >= request.FromDate && ro.CreatedAt <= request.ToDate)
            .ToListAsync(cancellationToken);

        response.KpiCards.InProgressCount = inProgressOrders.Count;
        response.KpiCards.CumulativeRevenue = revenueOrders.Sum(ro => ro.TotalAmount);

        if (completedOrders.Any())
        {
            var totalHours = completedOrders.Sum(ro => (ro.CompletedDate!.Value - ro.StartTime!.Value).TotalHours);
            response.KpiCards.AvgCompletionHours = totalHours / completedOrders.Count;
        }

        // 2. Urgent Alerts
        response.Alerts.OverdueTickets = await _context.RepairOrders
            .Where(ro => ro.Status != "Completed" && ro.ExpectedCompletionTime < now)
            .Select(ro => new OverdueTicketDto
            {
                TicketId = ro.Id,
                CustomerName = ro.CustomerName,
                ExpectedCompletionTime = ro.ExpectedCompletionTime ?? now,
                Status = ro.Status
            }).ToListAsync(cancellationToken);

        // Part Shortage: Check RepairOrderDetails of InProgress tickets
        var activeItems = await _context.RepairOrderDetails
            .Where(rod => rod.RepairOrder.Status == "InProgress" && rod.Type == "Part" && rod.ProductVariantId != null)
            .ToListAsync(cancellationToken);

        foreach (var item in activeItems)
        {
            // Mock logic: In real world, you'd check a real Inventory/Stock table.
            // Here we simulate check against InputInfos (assuming InputInfo represents stock)
            var stock = await _context.InputInfos
                .Where(ii => ii.ProductVariantColorId != null && ii.Count > 0)
                .SumAsync(ii => ii.Count ?? 0, cancellationToken);

            if (stock < item.Count)
            {
                response.Alerts.PartShortages.Add(new PartShortageDto
                {
                    TicketId = item.RepairOrderId,
                    PartName = item.ProductVariant?.ProductVariantColor?.ColorName ?? "Unknown Part",
                    RequiredQuantity = item.Count,
                    AvailableQuantity = stock
                });
            }
        }

        // 3. Analytics
        var workshopRev = revenueOrders.Sum(ro => ro.TotalAmount);
        var retailRev = await _context.OutputOrders
            .Where(o => o.CreatedAt >= request.FromDate && o.CreatedAt <= request.ToDate)
            .SumAsync(o => o.Total, cancellationToken);

        response.Analytics.RevenueComparison = new RevenueComparison
        {
            WorkshopRevenue = workshopRev,
            RetailRevenue = retailRev
        };

        response.Analytics.RevenueSources.Add(new RevenueSourceDto
        {
            Source = "Labor",
            Amount = revenueOrders.Sum(ro => ro.LaborCost)
        });
        response.Analytics.RevenueSources.Add(new RevenueSourceDto
        {
            Source = "Parts",
            Amount = revenueOrders.Sum(ro => ro.PartsCost)
        });

        // 4. Productivity
        var employees = await _context.EmployeeProfiles.ToListAsync(cancellationToken);
        foreach (var emp in employees)
        {
            var currentOrder = inProgressOrders.FirstOrDefault(ro => ro.TechnicianId == emp.Id);
            response.Productivity.TechnicianStatuses.Add(new TechnicianStatusDto
            {
                TechnicianName = emp.User?.FullName ?? "Unknown",
                Status = currentOrder != null ? "Busy" : "Idle",
                CurrentTicketId = currentOrder?.Id
            });

            var completedCount = completedOrders.Count(ro => ro.TechnicianId == emp.Id);
            var empRev = completedOrders.Where(ro => ro.TechnicianId == emp.Id).Sum(ro => ro.TotalAmount);

            response.Productivity.TechnicianRankings.Add(new TechnicianRankingDto
            {
                TechnicianName = emp.User?.FullName ?? "Unknown",
                CompletedTickets = completedCount,
                TotalRevenue = empRev,
                ComplaintRate = 0.0 // Mocked: would normally come from a feedback table
            });
        }

        return response;
    }
}
