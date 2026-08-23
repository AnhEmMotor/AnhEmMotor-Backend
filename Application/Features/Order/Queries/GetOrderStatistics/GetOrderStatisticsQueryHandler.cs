using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Order.Queries.GetOrderStatistics;

public class GetOrderStatisticsQueryHandler(
    IOutputReadRepository outputRepository,
    IReturnRequestReadRepository returnRequestRepository) : IRequestHandler<GetOrderStatisticsQuery, Result<OrderStatisticsResponse>>
{
    public async Task<Result<OrderStatisticsResponse>> Handle(
        GetOrderStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var orders = (await outputRepository.GetOrderStatisticsDataAsync(cancellationToken).ConfigureAwait(false)).ToList();
        var returnRequestCount = await returnRequestRepository.CountAsync(cancellationToken).ConfigureAwait(false);
        var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var pendingStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            OrderStatus.Pending,
            OrderStatus.WaitingDeposit,
            OrderStatus.WaitingInstallment,
            OrderStatus.PaidProcessing,
            OrderStatus.DepositPaid,
            OrderStatus.ConfirmedCod,
            OrderStatus.Delivering,
            OrderStatus.WaitingPickup,
            OrderStatus.Refunding
        };
        var pendingOrders = orders.Count(o => o.StatusId != null && pendingStatuses.Contains(o.StatusId));
        var slaDelayed = orders.Count(o =>
            o.StatusId != null && pendingStatuses.Contains(o.StatusId) && o.CreatedAt < today.AddDays(-1));
        var paymentErrors = orders.Count(o =>
            string.Equals(o.PaymentStatus, OrderPaymentStatus.Failed, StringComparison.OrdinalIgnoreCase) ||
            (string.Equals(o.StatusId, OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase) && o.PaidAmount > 0));
        var completedToday = orders.Count(o =>
            string.Equals(o.StatusId, OrderStatus.Completed, StringComparison.OrdinalIgnoreCase) &&
            (o.LastStatusChangedAt ?? o.CreatedAt) >= today);
        var hourlyData = orders
            .Where(o => o.CreatedAt >= today)
            .GroupBy(o => o.CreatedAt!.Value.Hour)
            .OrderBy(g => g.Key)
            .Select(g => new HourlyOrderData { Hour = $"{g.Key:00}:00", Count = g.Count() })
            .ToList();
        var exceptionOrders = orders
            .Where(o => o.StatusId != null && pendingStatuses.Contains(o.StatusId) && o.CreatedAt < today)
            .OrderByDescending(o => o.CreatedAt)
            .Take(20)
            .Select(
                o => new ExceptionOrder
                {
                    Id = o.Id,
                    CustomerName = o.CustomerName ?? string.Empty,
                    Issue = "Quá hạn chờ xử lý",
                    Type = "sla_delay"
                })
            .ToList();
        var response = new OrderStatisticsResponse
        {
            PendingOrders = pendingOrders,
            SlaDelayed = slaDelayed,
            PaymentErrors = paymentErrors,
            ReturnRequests = returnRequestCount,
            CompletedToday = completedToday,
            TargetToday = 60,
            HourlyData = hourlyData,
            ExceptionOrders = exceptionOrders
        };
        return Result<OrderStatisticsResponse>.Success(response);
    }
}
