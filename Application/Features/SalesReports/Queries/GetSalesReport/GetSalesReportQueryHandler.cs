using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.SalesReports.Queries.GetSalesReport;

public class GetSalesReportQueryHandler(IOutputReadRepository outputReadRepository)
    : IRequestHandler<GetSalesReportQuery, Result<SalesReportResponse>>
{
    public async Task<Result<SalesReportResponse>> Handle(
        GetSalesReportQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await outputReadRepository
            .GetAllAsync(cancellationToken, Domain.Constants.DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        var orderList = orders.ToList();

        var totalOrders = orderList.Count;
        var totalRevenue = orderList.Sum(o => o.Total);
        var totalQuantitySold = orderList.Sum(o => o.OutputInfos.Sum(oi => oi.Count ?? 0));
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var pendingCount = CountByStatus(orderList, OrderStatus.Pending);
        var confirmedCount = CountByStatus(orderList, OrderStatus.ConfirmedCod)
            + CountByStatus(orderList, OrderStatus.PaidProcessing)
            + CountByStatus(orderList, OrderStatus.Delivering)
            + CountByStatus(orderList, OrderStatus.WaitingPickup)
            + CountByStatus(orderList, OrderStatus.Refunding)
            + CountByStatus(orderList, OrderStatus.Refunded)
            + CountByStatus(orderList, OrderStatus.InstallmentApproved);
        var completedCount = CountByStatus(orderList, OrderStatus.Completed);
        var cancelledCount = CountByStatus(orderList, OrderStatus.Cancelled);
        var refundedCount = CountByStatus(orderList, OrderStatus.Refunded);
        var deliveringCount = CountByStatus(orderList, OrderStatus.Delivering);
        var depositPaidCount = CountByStatus(orderList, OrderStatus.DepositPaid);
        var waitingDepositCount = CountByStatus(orderList, OrderStatus.WaitingDeposit);

        var items = orderList
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new SalesReportItem
            {
                OrderId = o.Id,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                StatusId = o.StatusId,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                Total = o.Total,
                PaidAmount = o.PaidAmount,
                CreatedAt = o.CreatedAt,
                LastStatusChangedAt = o.LastStatusChangedAt,
            })
            .ToList();

        var response = new SalesReportResponse
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TotalQuantitySold = totalQuantitySold,
            AverageOrderValue = averageOrderValue,
            PendingCount = pendingCount,
            ConfirmedCount = confirmedCount,
            CompletedCount = completedCount,
            CancelledCount = cancelledCount,
            RefundedCount = refundedCount,
            DeliveringCount = deliveringCount,
            DepositPaidCount = depositPaidCount,
            WaitingDepositCount = waitingDepositCount,
            Items = items,
        };

        return Result<SalesReportResponse>.Success(response);
    }

    private static int CountByStatus(List<Domain.Entities.Output> orders, string status)
    {
        return orders.Count(o => string.Equals(o.StatusId, status, StringComparison.OrdinalIgnoreCase));
    }
}
