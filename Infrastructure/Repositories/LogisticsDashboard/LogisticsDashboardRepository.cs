using Application.ApiContracts.Logistics.Responses;
using Application.Features.Logistics.Queries.GetLogisticsDashboard;
using Application.Interfaces.Repositories.LogisticsDashboard;
using Domain.Constants.Order;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.LogisticsDashboard;

public class LogisticsDashboardRepository : ILogisticsDashboardRepository
{
    private readonly ApplicationDBContext _context;

    public LogisticsDashboardRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<LogisticsDashboardResponse> GetDashboardAsync(DateTime fromDate, CancellationToken cancellationToken)
    {
        var response = new LogisticsDashboardResponse();

        // 1. FulfillmentWorkload: Số đơn đang giao (Shipping)
        var workload = await _context.Shipments
            .Where(s => s.Status == Domain.Enums.ParcelDeliveryStatus.Shipping && s.DeliveredAt == null)
            .CountAsync(cancellationToken);

        // 2. PendingUnreconciledCod: Tổng tiền COD chờ đối soát của các bưu kiện
        var pendingCod = await _context.Shipments
            .Where(s => s.Status == Domain.Enums.ParcelDeliveryStatus.Shipping && s.DeliveredAt == null)
            .SumAsync(s => s.CodAmount, cancellationToken);

        // 3. OtifRate: Tỷ lệ giao hàng thành công (Completed / Total Finished)
        var completedShipments = await _context.Shipments
            .Where(s => (s.Status == Domain.Enums.ParcelDeliveryStatus.Completed || (s.Status == Domain.Enums.ParcelDeliveryStatus.Shipping && s.DeliveredAt != null)) && s.CreatedAt >= fromDate)
            .CountAsync(cancellationToken);
            
        // 4. ReturnsClaimsRate: Tỷ lệ hoàn/hủy (Returned / Total Finished)
        var returnedShipments = await _context.Shipments
            .Where(s => s.Status == Domain.Enums.ParcelDeliveryStatus.Returned && s.CreatedAt >= fromDate)
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
