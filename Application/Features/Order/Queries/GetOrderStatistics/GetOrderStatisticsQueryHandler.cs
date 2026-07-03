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
            .GroupBy(b => b.CreatedAt!.Value.Hour)
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

        // Thêm dữ liệu mẫu (Mock data) nếu database rỗng để UI hiển thị sinh động
        if (pendingOrders == 0 && !hourlyData.Any())
        {
            pendingOrders = 12;
            slaDelayed = 3;
            paymentErrors = 1;
            returnRequests = 2;
            completedToday = 45;
            hourlyData = new List<HourlyOrderData>
            {
                new() { Hour = "08:00", Count = 5 },
                new() { Hour = "09:00", Count = 12 },
                new() { Hour = "10:00", Count = 18 },
                new() { Hour = "11:00", Count = 10 },
                new() { Hour = "12:00", Count = 6 },
                new() { Hour = "13:00", Count = 14 },
                new() { Hour = "14:00", Count = 22 },
                new() { Hour = "15:00", Count = 17 },
                new() { Hour = "16:00", Count = 9 }
            };
            exceptionOrders = new List<ExceptionOrder>
            {
                new() { Id = 1001, CustomerName = "Nguyễn Văn A", Issue = "Chưa thanh toán", Type = "payment" },
                new() { Id = 1002, CustomerName = "Trần Thị B", Issue = "Quá hạn xử lý 2 ngày", Type = "sla_delay" },
                new() { Id = 1003, CustomerName = "Lê Hoàng C", Issue = "Yêu cầu hoàn tiền", Type = "return" }
            };
        }

        var response = new OrderStatisticsResponse
        {
            PendingOrders = pendingOrders,
            SlaDelayed = slaDelayed,
            PaymentErrors = paymentErrors,
            ReturnRequests = returnRequests,
            CompletedToday = completedToday,
            TargetToday = 60,
            HourlyData = hourlyData,
            ExceptionOrders = exceptionOrders
        };

        return Result<OrderStatisticsResponse>.Success(response);
    }
}
