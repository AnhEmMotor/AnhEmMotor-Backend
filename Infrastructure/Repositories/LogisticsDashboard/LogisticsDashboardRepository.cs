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
        return response;
    }
}
