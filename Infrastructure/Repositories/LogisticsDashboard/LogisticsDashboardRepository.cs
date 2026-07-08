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

        // 1. FulfillmentWorkload: Số đơn đang giao (Delivering)
        var workload = await _context.OutputOrders
            .Where(o => o.StatusId == OrderStatus.Delivering)
            .CountAsync(cancellationToken);

        // 2. PendingUnreconciledCod: Tổng tiền COD chờ đối soát của các bưu kiện
        // Use OutputOrders directly to find unpaid amounts for delivering orders
        var pendingOrders = await _context.OutputOrders
            .Include(o => o.OutputInfos)
            .Where(o => o.StatusId == OrderStatus.Delivering)
            .ToListAsync(cancellationToken);

        decimal pendingCod = pendingOrders.Sum(o => o.Total - (o.PaidAmount ?? 0));

        // 3. OtifRate (On Time In Full): Tỷ lệ giao đúng hạn (Giả lập tính toán đơn giản)
        double otif = 95.0; // Mocked value since detailed delivery time tracking is delegated to GHTK

        // 4. ReturnsClaimsRate: Tỷ lệ hoàn/hủy
        var totalOrders = await _context.OutputOrders
            .Where(o => o.CreatedAt >= fromDate)
            .CountAsync(cancellationToken);
            
        var returnedOrders = await _context.OutputOrders
            .Where(o => (o.StatusId == OrderStatus.Refunding || o.StatusId == OrderStatus.Refunded) && o.CreatedAt >= fromDate)
            .CountAsync(cancellationToken);

        double returnRate = totalOrders > 0 ? Math.Round((double)returnedOrders / totalOrders * 100, 2) : 0.0;

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
