using Application.Common.Models;
using Application.Features.Order.Queries.GetOrderStatistics;
using Application.Interfaces.Repositories.Booking;
using Domain.Entities;
using MediatR;

namespace Application.Features.Order.Queries.GetOrderStatistics;

public class GetOrderStatisticsQueryHandler(IBookingReadRepository bookingRepository)
    : IRequestHandler<GetOrderStatisticsQuery, Result<OrderStatisticsResponse>>
{
    public async Task<Result<OrderStatisticsResponse>> Handle(
        GetOrderStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var bookings = await bookingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var today = DateTime.UtcNow.Date;

        var pendingOrders = bookings.Count(b => b.Status == "Pending");
        var slaDelayed = bookings.Count(b => b.Status == "Pending" && b.PreferredDate < today.AddDays(-1));
        var paymentErrors = bookings.Count(b => b.Status == "Cancelled");
        var returnRequests = 0;
        var completedToday = bookings.Count(b => b.Status == "Confirmed" && b.CreatedAt >= today);

        var hourlyData = bookings
            .Where(b => b.CreatedAt >= today)
            .GroupBy(b => b.CreatedAt.Value.Hour)
            .OrderBy(g => g.Key)
            .Select(g => new HourlyOrderData
            {
                Hour = $"{g.Key:00}:00",
                Count = g.Count()
            })
            .ToList();

        var exceptionOrders = bookings
            .Where(b => b.Status == "Pending" && b.PreferredDate < today)
            .OrderByDescending(b => b.CreatedAt)
            .Take(20)
            .Select(b => new ExceptionOrder
            {
                Id = b.Id,
                CustomerName = b.FullName,
                Issue = "Quá hạn chờ xử lý",
                Type = "sla_delay"
            })
            .ToList();

        var response = new OrderStatisticsResponse
        {
            PendingOrders = pendingOrders,
            SlaDelayed = slaDelayed,
            PaymentErrors = paymentErrors,
            ReturnRequests = returnRequests,
            CompletedToday = completedToday,
            TargetToday = 50,
            HourlyData = hourlyData,
            ExceptionOrders = exceptionOrders
        };

        return Result<OrderStatisticsResponse>.Success(response);
    }
}
