using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.InventoryReceiptInfo;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.RepairOrder;
using Domain.Constants;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Statistical.Queries.GetWorkshopDashboard;

public class GetWorkshopDashboardQueryHandler(
    IRepairOrderReadRepository repairOrderReadRepository,
    IOutputReadRepository outputReadRepository,
    IEmployeeReadRepository employeeReadRepository,
    IInventoryReceiptInfoReadRepository inventoryReceiptInfoReadRepository) : IRequestHandler<GetWorkshopDashboardQuery, WorkshopDashboardResponse>
{
    public async Task<WorkshopDashboardResponse> Handle(
        GetWorkshopDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var response = new WorkshopDashboardResponse();
        var allOrders = await repairOrderReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var inProgressOrders = allOrders.Where(ro => string.Compare(ro.Status, RepairOrderStatus.InProgress) == 0)
            .ToList();
        var completedOrders = allOrders.Where(
            ro => string.Compare(ro.Status, RepairOrderStatus.Completed) == 0 &&
                ro.StartTime != null &&
                ro.CompletedDate != null)
            .ToList();
        var revenueOrders = allOrders.Where(ro => ro.CreatedAt >= request.FromDate && ro.CreatedAt <= request.ToDate)
            .ToList();
        response.KpiCards.InProgressCount = inProgressOrders.Count;
        response.KpiCards.CumulativeRevenue = revenueOrders.Sum(ro => ro.TotalAmount);
        if (completedOrders.Count != 0)
        {
            var totalHours = completedOrders.Sum(ro => (ro.CompletedDate!.Value - ro.StartTime!.Value).TotalHours);
            response.KpiCards.AvgCompletionHours = totalHours / completedOrders.Count;
        }
        response.Alerts.OverdueTickets = [.. allOrders
            .Where(ro => string.Compare(ro.Status, RepairOrderStatus.Completed) != 0 && ro.ExpectedCompletionTime < now)
            .Select(
                ro => new OverdueTicketResponse
                {
                    TicketId = ro.Id,
                    CustomerName = ro.CustomerName,
                    ExpectedCompletionTime = ro.ExpectedCompletionTime ?? now,
                    Status = ro.Status
                })];
        var activeItems = allOrders
            .SelectMany(ro => ro.Details)
            .Where(
                rod => string.Compare(rod.RepairOrder.Status, RepairOrderStatus.InProgress) == 0 &&
                    string.Compare(rod.Type, RepairOrderDetailType.Part) == 0 &&
                    rod.ProductVariantId != null)
            .ToList();
        foreach (var item in activeItems)
        {
            var stock = (await inventoryReceiptInfoReadRepository.GetFinishedInventoryReceiptInfosByVariantIdAsync(
                item.ProductVariantId!.Value,
                cancellationToken)
                .ConfigureAwait(false))
                .Where(ii => ii.Count > 0)
                .Sum(ii => ii.Count ?? 0);
            if (stock < item.Count)
            {
                response.Alerts.PartShortages
                    .Add(
                        new PartShortageResponse
                        {
                            TicketId = item.RepairOrderId,
                            PartName = item.ProductVariant?.Product?.Name ?? "Unknown Part",
                            RequiredQuantity = item.Count,
                            AvailableQuantity = stock
                        });
            }
        }
        var workshopRev = revenueOrders.Sum(ro => ro.TotalAmount);
        var retailRev = (await outputReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .Where(o => o.CreatedAt >= request.FromDate && o.CreatedAt <= request.ToDate)
            .Sum(o => o.Total);
        response.Analytics.RevenueComparison = new RevenueComparison
        {
            WorkshopRevenue = workshopRev,
            RetailRevenue = retailRev
        };
        response.Analytics.RevenueSources
            .Add(
                new RevenueSourceResponse
                {
                    Source = WorkshopDashboardConstants.RevenueSource.Labor,
                    Amount = revenueOrders.Sum(ro => ro.LaborCost)
                });
        response.Analytics.RevenueSources
            .Add(
                new RevenueSourceResponse
                {
                    Source = WorkshopDashboardConstants.RevenueSource.Parts,
                    Amount = revenueOrders.Sum(ro => ro.PartsCost)
                });
        var employees = await employeeReadRepository.GetAllWithUsersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var emp in employees)
        {
            var currentOrder = inProgressOrders.FirstOrDefault(ro => ro.TechnicianId == emp.Id);
            response.Productivity.TechnicianStatuses
                .Add(
                    new TechnicianStatusResponse
                    {
                        TechnicianName = emp.User?.FullName ?? "Unknown",
                        Status =
                            currentOrder != null
                                    ? WorkshopDashboardConstants.TechnicianStatus.Busy
                                    : WorkshopDashboardConstants.TechnicianStatus.Idle,
                        CurrentTicketId = currentOrder?.Id
                    });
            var completedCount = completedOrders.Count(ro => ro.TechnicianId == emp.Id);
            var empRev = completedOrders.Where(ro => ro.TechnicianId == emp.Id).Sum(ro => ro.TotalAmount);
            response.Productivity.TechnicianRankings
                .Add(
                    new TechnicianRankingResponse
                    {
                        TechnicianName = emp.User?.FullName ?? "Unknown",
                        CompletedTickets = completedCount,
                        TotalRevenue = empRev,
                        ComplaintRate = 0.0
                    });
        }
        return response;
    }
}
