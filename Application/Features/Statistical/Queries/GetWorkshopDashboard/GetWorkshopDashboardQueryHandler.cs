using Application.Api.Contracts.Statistical.Responses;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.RepairOrder;
using Application.Interfaces.Repositories.HR.Employee;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Statistical.Queries.GetWorkshopDashboard;

public class GetWorkshopDashboardQueryHandler : IRequestHandler<GetWorkshopDashboardQuery, WorkshopDashboardResponse>
{
    private readonly IRepairOrderReadRepository _repairOrderReadRepository;
    private readonly IOutputReadRepository _outputReadRepository;
    private readonly IEmployeeReadRepository _employeeReadRepository;
    private readonly IInventoryReceiptInfoReadRepository _inventoryReceiptInfoReadRepository;

    public GetWorkshopDashboardQueryHandler(
        IRepairOrderReadRepository repairOrderReadRepository,
        IOutputReadRepository outputReadRepository,
        IEmployeeReadRepository employeeReadRepository,
        IInventoryReceiptInfoReadRepository inventoryReceiptInfoReadRepository)
    {
        _repairOrderReadRepository = repairOrderReadRepository;
        _outputReadRepository = outputReadRepository;
        _employeeReadRepository = employeeReadRepository;
        _inventoryReceiptInfoReadRepository = inventoryReceiptInfoReadRepository;
    }

    public async Task<WorkshopDashboardResponse> Handle(GetWorkshopDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var response = new WorkshopDashboardResponse();

        // 1. KPI Cards
        var allOrders = await _repairOrderReadRepository.GetAllAsync(cancellationToken);
        var inProgressOrders = allOrders.Where(ro => ro.Status == "InProgress").ToList();
        var completedOrders = allOrders.Where(ro => ro.Status == "Completed" && ro.StartTime != null && ro.CompletedDate != null).ToList();
        var revenueOrders = allOrders.Where(ro => ro.CreatedAt >= request.FromDate && ro.CreatedAt <= request.ToDate).ToList();

        response.KpiCards.InProgressCount = inProgressOrders.Count;
        response.KpiCards.CumulativeRevenue = revenueOrders.Sum(ro => ro.TotalAmount);

        if (completedOrders.Any())
        {
            var totalHours = completedOrders.Sum(ro => (ro.CompletedDate!.Value - ro.StartTime!.Value).TotalHours);
            response.KpiCards.AvgCompletionHours = totalHours / completedOrders.Count;
        }

        // 2. Urgent Alerts
        response.Alerts.OverdueTickets = allOrders
            .Where(ro => ro.Status != "Completed" && ro.ExpectedCompletionTime < now)
            .Select(ro => new OverdueTicketDto
            {
                TicketId = ro.Id,
                CustomerName = ro.CustomerName,
                ExpectedCompletionTime = ro.ExpectedCompletionTime ?? now,
                Status = ro.Status
            }).ToList();

        // Part Shortage: Check RepairOrderDetails of InProgress tickets
        var activeItems = allOrders
            .SelectMany(ro => ro.Details)
            .Where(rod => rod.RepairOrder.Status == "InProgress" && rod.Type == "Part" && rod.ProductVariantId != null)
            .ToList();

        foreach (var item in activeItems)
        {
            // Mock logic: count finished receipt stock for the variant.
            var stock = (await _inventoryReceiptInfoReadRepository.GetFinishedInventoryReceiptInfosByVariantIdAsync(item.ProductVariantId!.Value, cancellationToken))
                .Where(ii => ii.Count > 0)
                .Sum(ii => ii.Count ?? 0);

            if (stock < item.Count)
            {
                response.Alerts.PartShortages.Add(new PartShortageDto
                {
                    TicketId = item.RepairOrderId,
                PartName = item.ProductVariant?.Product?.Name ?? "Unknown Part",
                    RequiredQuantity = item.Count,
                    AvailableQuantity = stock
                });
            }
        }

        // 3. Analytics
        var workshopRev = revenueOrders.Sum(ro => ro.TotalAmount);
        var retailRev = (await _outputReadRepository.GetAllAsync(cancellationToken))
            .Where(o => o.CreatedAt >= request.FromDate && o.CreatedAt <= request.ToDate)
            .Sum(o => o.Total);

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
        var employees = await _employeeReadRepository.GetAllWithUsersAsync(cancellationToken);
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
