using Application.ApiContracts.Logistics.Responses;
using Application.Features.Logistics.Queries.GetLogisticsDashboard;
using Application.Interfaces.Repositories.LogisticsDashboard;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Infrastructure.Repositories.LogisticsDashboard;

public class LogisticsDashboardRepository : ILogisticsDashboardRepository
{
    private readonly ApplicationDBContext _context;

    public LogisticsDashboardRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<LogisticsDashboardResponse> GetDashboardAsync(
        DateTime fromDate,
        CancellationToken cancellationToken)
    {
        var response = new LogisticsDashboardResponse();
        var workload = await _context.Shipments
            .Where(s => s.Status == ParcelDeliveryStatus.Shipping && s.DeliveredAt == null)
            .CountAsync(cancellationToken);
        var pendingCod = await _context.Shipments
            .Where(s => s.Status == ParcelDeliveryStatus.Shipping && s.DeliveredAt == null)
            .SumAsync(s => s.CodAmount, cancellationToken);
        var completedShipments = await _context.Shipments
            .Where(
                s => (s.Status == ParcelDeliveryStatus.Completed ||
                        (s.Status == ParcelDeliveryStatus.Shipping && s.DeliveredAt != null)) &&
                    s.CreatedAt >= fromDate)
            .CountAsync(cancellationToken);
        var returnedShipments = await _context.Shipments
            .Where(s => s.Status == ParcelDeliveryStatus.Returned && s.CreatedAt >= fromDate)
            .CountAsync(cancellationToken);
        var totalFinished = completedShipments + returnedShipments;
        double otif = totalFinished > 0 ? (double)completedShipments / totalFinished : 0.0;
        double returnRate = totalFinished > 0 ? (double)returnedShipments / totalFinished : 0.0;
        response.Summary = new LogisticsDashboardSummaryResponse
        {
            FulfillmentWorkload = workload,
            FulfillmentWorkloadIsOverload = workload > 50,
            PendingUnreconciledCod = pendingCod,
            OtifRate = otif,
            ReturnsClaimsRate = returnRate
        };

        // Fulfillment Funnel
        var allShipments = await _context.Shipments
            .Where(s => s.CreatedAt >= fromDate)
            .ToListAsync(cancellationToken);
            
        response.FulfillmentFunnel["total"] = allShipments.Count;
        response.FulfillmentFunnel["shipping"] = allShipments.Count(s => s.Status == ParcelDeliveryStatus.Shipping);
        response.FulfillmentFunnel["completed"] = allShipments.Count(s => s.Status == ParcelDeliveryStatus.Completed);
        response.FulfillmentFunnel["returned"] = allShipments.Count(s => s.Status == ParcelDeliveryStatus.Returned);

        // Trends (Group by day)
        var trends = allShipments
            .Where(s => s.CreatedAt != null)
            .GroupBy(s => s.CreatedAt!.Value.Date)
            .OrderBy(g => g.Key)
            .Select(g => new LogisticsTrendPointResponse
            {
                DayLabel = g.Key.ToString("dd/MM"),
                DeliveredCount = g.Count(s => s.Status == ParcelDeliveryStatus.Completed || s.DeliveredAt != null),
                ShippingCost = g.Sum(s => s.ShippingCost)
            })
            .ToList();
        response.Trends = trends;

        // Carrier Scorecard
        var carrierGroups = allShipments
            .Where(s => !string.IsNullOrEmpty(s.Carrier))
            .GroupBy(s => s.Carrier)
            .Select(g =>
            {
                var delivered = g.Where(s => s.Status == ParcelDeliveryStatus.Completed || s.DeliveredAt != null).ToList();
                var returned = g.Where(s => s.Status == ParcelDeliveryStatus.Returned).ToList();
                double avgDays = delivered.Any(d => d.DeliveredAt.HasValue && d.CreatedAt.HasValue)
                    ? delivered.Where(d => d.DeliveredAt.HasValue && d.CreatedAt.HasValue).Average(d => (d.DeliveredAt!.Value - d.CreatedAt!.Value).TotalDays)
                    : 0;
                return new CarrierScoreRowResponse
                {
                    Carrier = g.Key,
                    DeliveredCount = delivered.Count,
                    AvgDeliveryDays = Math.Round(avgDays, 1),
                    AvgShippingCostPerOrder = g.Any() ? g.Average(s => s.ShippingCost) : 0,
                    ReturnsRatio = g.Any() ? (double)returned.Count / g.Count() : 0
                };
            })
            .ToList();
        response.CarrierScorecard = carrierGroups;

        // Exceptions (Mocked for stuck orders)
        var stuckOrders = allShipments
            .Where(s => s.Status == ParcelDeliveryStatus.Shipping && s.CreatedAt.HasValue && (DateTimeOffset.UtcNow - s.CreatedAt.Value).TotalDays > 3)
            .Select(s => new LogisticsExceptionRowResponse
            {
                Type = "Overdue",
                TrackingNumber = s.TrackingNumber ?? s.Id.ToString(),
                Message = $"Đơn hàng quá hạn giao ({Math.Round((DateTimeOffset.UtcNow - s.CreatedAt!.Value).TotalDays)} ngày)",
                CreatedAt = s.CreatedAt.Value.UtcDateTime
            })
            .Take(5)
            .ToList();
        response.Exceptions = stuckOrders;

        return response;
    }
}
