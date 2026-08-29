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
        var allOrders = (await outputRepository.GetOrderStatisticsDataAsync(cancellationToken).ConfigureAwait(false)).ToList();
        var returnRequestCount = await returnRequestRepository.CountAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
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

        // Workload & Alerts (calculated on all current active state)
        var pendingOrders = allOrders.Count(o => o.StatusId != null && pendingStatuses.Contains(o.StatusId));
        var slaDelayed = allOrders.Count(o =>
            o.StatusId != null && pendingStatuses.Contains(o.StatusId) && o.CreatedAt.HasValue && o.CreatedAt.Value < now.AddHours(-24));
        var paymentErrors = allOrders.Count(o =>
            string.Equals(o.PaymentStatus, OrderPaymentStatus.Failed, StringComparison.OrdinalIgnoreCase) ||
            (string.Equals(o.StatusId, OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase) && (o.PaidAmount ?? 0) > 0));
        var completedToday = allOrders.Count(o =>
            string.Equals(o.StatusId, OrderStatus.Completed, StringComparison.OrdinalIgnoreCase) &&
            (o.LastStatusChangedAt ?? o.CreatedAt) >= today);

        // Apply filters
        var filteredQuery = allOrders.AsEnumerable();

        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.Date;
            filteredQuery = filteredQuery.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= start);
        }

        if (request.EndDate.HasValue)
        {
            var end = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            filteredQuery = filteredQuery.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value <= end);
        }

        if (!string.IsNullOrEmpty(request.Channel))
        {
            if (request.Channel.Equals("online", StringComparison.OrdinalIgnoreCase))
            {
                filteredQuery = filteredQuery.Where(o => o.CreatedBy == null || o.LeadId != null);
            }
            else if (request.Channel.Equals("offline", StringComparison.OrdinalIgnoreCase))
            {
                filteredQuery = filteredQuery.Where(o => o.CreatedBy != null && o.LeadId == null);
            }
        }

        if (!string.IsNullOrEmpty(request.PaymentMethod))
        {
            filteredQuery = filteredQuery.Where(o =>
                string.Equals(o.PaymentMethod, request.PaymentMethod, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(request.StatusId))
        {
            filteredQuery = filteredQuery.Where(o =>
                string.Equals(o.StatusId, request.StatusId, StringComparison.OrdinalIgnoreCase));
        }

        var filteredOrders = filteredQuery.ToList();

        // High level KPI calculations
        int totalOrders = filteredOrders.Count;
        decimal totalRevenue = filteredOrders
            .Where(o => !string.Equals(o.StatusId, OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
            .Sum(o => (o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0) + (o.ShippingFee ?? 0));
        decimal aov = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 0) : 0m;
        int cancelledCount = filteredOrders.Count(o => string.Equals(o.StatusId, OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase));
        double cancellationRate = totalOrders > 0 ? Math.Round((double)cancelledCount / totalOrders * 100, 1) : 0;

        // 1. Hourly Trend Data (24 hours full baseline)
        var targetDayStart = request.StartDate.HasValue ? request.StartDate.Value.Date : today;
        var targetDayEnd = targetDayStart.AddDays(1);
        var hourlyOrders = allOrders.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value >= targetDayStart && o.CreatedAt.Value < targetDayEnd).ToList();

        var hourlyDict = new Dictionary<int, (int Count, decimal Revenue)>();
        for (int h = 0; h < 24; h++)
        {
            hourlyDict[h] = (0, 0m);
        }

        foreach (var o in hourlyOrders)
        {
            int h = o.CreatedAt!.Value.Hour;
            decimal orderTotal = (o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0) + (o.ShippingFee ?? 0);
            var cur = hourlyDict[h];
            hourlyDict[h] = (cur.Count + 1, cur.Revenue + (!string.Equals(o.StatusId, OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase) ? orderTotal : 0));
        }

        var hourlyData = hourlyDict.Select(kvp => new HourlyOrderData
        {
            Hour = $"{kvp.Key:00}:00",
            Count = kvp.Value.Count,
            Revenue = kvp.Value.Revenue
        }).OrderBy(x => x.Hour).ToList();

        // 2. Daily Trend Data (if range is more than 1 day or for period analytics)
        var dailyData = filteredOrders
            .Where(o => o.CreatedAt.HasValue)
            .GroupBy(o => o.CreatedAt!.Value.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyOrderData
            {
                Date = g.Key.ToString("dd/MM"),
                Count = g.Count(),
                Revenue = g.Where(o => !string.Equals(o.StatusId, OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
                           .Sum(o => (o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0) + (o.ShippingFee ?? 0))
            })
            .ToList();

        // 3. Status Distribution
        var statusData = filteredOrders
            .Where(o => !string.IsNullOrEmpty(o.StatusId))
            .GroupBy(o => o.StatusId!)
            .Select(g => new OrderStatusStatData
            {
                StatusId = g.Key,
                StatusName = OrderStatus.GetDisplayName(g.Key),
                Count = g.Count(),
                TotalAmount = g.Sum(o => (o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0) + (o.ShippingFee ?? 0))
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        // 4. Delivery Method Distribution
        int homeDeliveryCount = filteredOrders.Count(o => (o.ShippingFee.HasValue && o.ShippingFee.Value > 0) || !string.IsNullOrEmpty(o.WardCode));
        int showroomPickupCount = filteredOrders.Count - homeDeliveryCount;
        var deliveryMethodData = new List<DeliveryMethodStatData>();
        if (totalOrders > 0)
        {
            deliveryMethodData.Add(new DeliveryMethodStatData
            {
                Method = "Giao tận nhà",
                Count = homeDeliveryCount,
                Percentage = Math.Round((double)homeDeliveryCount / totalOrders * 100, 1)
            });
            deliveryMethodData.Add(new DeliveryMethodStatData
            {
                Method = "Nhận tại showroom",
                Count = showroomPickupCount,
                Percentage = Math.Round((double)showroomPickupCount / totalOrders * 100, 1)
            });
        }
        else
        {
            deliveryMethodData.Add(new DeliveryMethodStatData { Method = "Giao tận nhà", Count = 0, Percentage = 0 });
            deliveryMethodData.Add(new DeliveryMethodStatData { Method = "Nhận tại showroom", Count = 0, Percentage = 0 });
        }

        // 5. Payment Method Distribution
        var paymentMethodData = filteredOrders
            .GroupBy(o => string.IsNullOrWhiteSpace(o.PaymentMethod) ? "Khác" : o.PaymentMethod.Trim())
            .Select(g => new PaymentMethodStatData
            {
                Method = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(o => (o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0) + (o.ShippingFee ?? 0))
            })
            .OrderByDescending(p => p.Count)
            .ToList();

        // 6. Channel Distribution
        int onlineCount = filteredOrders.Count(o => o.CreatedBy == null || o.LeadId != null);
        int offlineCount = filteredOrders.Count - onlineCount;
        var channelData = new List<ChannelStatData>
        {
            new()
            {
                Channel = "Online",
                Count = onlineCount,
                TotalAmount = filteredOrders.Where(o => o.CreatedBy == null || o.LeadId != null)
                                            .Sum(o => (o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0) + (o.ShippingFee ?? 0))
            },
            new()
            {
                Channel = "Showroom",
                Count = offlineCount,
                TotalAmount = filteredOrders.Where(o => o.CreatedBy != null && o.LeadId == null)
                                            .Sum(o => (o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0) + (o.ShippingFee ?? 0))
            }
        };

        // 7. Critical / Exception Orders
        var exceptionList = new List<ExceptionOrder>();
        foreach (var o in allOrders.OrderByDescending(o => o.CreatedAt))
        {
            string? issue = null;
            string type = "pending";

            bool isSlaDelayed = o.StatusId != null && pendingStatuses.Contains(o.StatusId) && o.CreatedAt.HasValue && o.CreatedAt.Value < now.AddHours(-24);
            bool isPaymentError = string.Equals(o.PaymentStatus, OrderPaymentStatus.Failed, StringComparison.OrdinalIgnoreCase);
            bool isCancelRefund = string.Equals(o.StatusId, OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase) && (o.PaidAmount ?? 0) > 0;
            bool isRefunding = string.Equals(o.StatusId, OrderStatus.Refunding, StringComparison.OrdinalIgnoreCase);
            bool isNewPending = string.Equals(o.StatusId, OrderStatus.Pending, StringComparison.OrdinalIgnoreCase) && o.CreatedAt.HasValue && o.CreatedAt.Value < now.AddHours(-2);
            bool isWaitingDepositExpired = string.Equals(o.StatusId, OrderStatus.WaitingDeposit, StringComparison.OrdinalIgnoreCase) &&
                ((o.PaymentExpiredAt.HasValue && o.PaymentExpiredAt.Value < now) || (o.CreatedAt.HasValue && o.CreatedAt.Value < now.AddHours(-12)));

            if (isSlaDelayed)
            {
                issue = "Quá hạn SLA xử lý (> 24h)";
                type = "sla";
            }
            else if (isPaymentError)
            {
                issue = "Thanh toán thất bại / Lỗi đối soát";
                type = "payment";
            }
            else if (isCancelRefund || isRefunding)
            {
                issue = "Đã hủy đơn - Cần hoàn tiền cọc";
                type = "payment";
            }
            else if (isWaitingDepositExpired)
            {
                issue = "Chờ đặt cọc quá hạn";
                type = "pending";
            }
            else if (isNewPending)
            {
                issue = "Đơn mới chờ duyệt (> 2h)";
                type = "pending";
            }

            if (issue != null)
            {
                decimal total = (o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0) + (o.ShippingFee ?? 0);
                string waitTime = FormatWaitTime(o.CreatedAt, now);

                exceptionList.Add(new ExceptionOrder
                {
                    Id = o.Id,
                    OrderCode = $"ORD-{o.Id:D5}",
                    CustomerName = !string.IsNullOrWhiteSpace(o.CustomerName) ? o.CustomerName : "Khách vãng lai",
                    CustomerPhone = o.CustomerPhone ?? "-",
                    TotalAmount = total,
                    PaidAmount = o.PaidAmount ?? 0,
                    StatusId = o.StatusId ?? string.Empty,
                    StatusName = OrderStatus.GetDisplayName(o.StatusId ?? string.Empty),
                    PaymentStatus = o.PaymentStatus ?? string.Empty,
                    PaymentMethod = !string.IsNullOrWhiteSpace(o.PaymentMethod) ? o.PaymentMethod : "Chưa chọn",
                    Issue = issue,
                    Type = type,
                    WaitTime = waitTime,
                    CreatedAt = o.CreatedAt,
                    DeliveryType = (o.ShippingFee.HasValue && o.ShippingFee.Value > 0) ? "Giao tận nhà" : "Nhận tại showroom"
                });
            }
        }

        var response = new OrderStatisticsResponse
        {
            PendingOrders = pendingOrders,
            SlaDelayed = slaDelayed,
            PaymentErrors = paymentErrors,
            ReturnRequests = returnRequestCount,
            CompletedToday = completedToday,
            TargetToday = 60,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            AverageOrderValue = aov,
            CancellationRate = cancellationRate,
            HourlyData = hourlyData,
            DailyData = dailyData,
            StatusData = statusData,
            DeliveryMethodData = deliveryMethodData,
            PaymentMethodData = paymentMethodData,
            ChannelData = channelData,
            ExceptionOrders = exceptionList.Take(50).ToList()
        };

        return Result<OrderStatisticsResponse>.Success(response);
    }

    private static string FormatWaitTime(DateTimeOffset? createdAt, DateTimeOffset now)
    {
        if (!createdAt.HasValue) return "-";
        var span = now - createdAt.Value;
        if (span.TotalMinutes < 60)
        {
            return $"{(int)Math.Max(1, span.TotalMinutes)} phút trước";
        }
        if (span.TotalHours < 24)
        {
            return $"{(int)span.TotalHours} giờ trước";
        }
        return $"{(int)span.TotalDays} ngày trước";
    }
}

