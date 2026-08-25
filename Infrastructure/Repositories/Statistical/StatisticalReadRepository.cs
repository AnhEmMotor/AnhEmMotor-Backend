using Application.Api.Contracts.Statistical.Responses;
using Application.ApiContracts.Admin.Analytics;
using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using Domain.Constants.InventoryReceipt;
using Domain.Constants.Order;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using WorkshopRevenueComparison = Application.Api.Contracts.Statistical.Responses.RevenueComparison;

namespace Infrastructure.Repositories.Statistical;

public class StatisticalReadRepository(ApplicationDBContext context) : IStatisticalReadRepository
{
    public async Task<WorkshopDashboardResponse> GetWorkshopDashboardOverviewAsync(
        string from,
        string to,
        CancellationToken cancellationToken)
    {
        DateTimeOffset fromDate;
        if (!DateTimeOffset.TryParse(from, out fromDate))
        {
            fromDate = DateTimeOffset.UtcNow.AddDays(-30);
        } else
        {
            fromDate = fromDate.ToUniversalTime();
        }
        DateTimeOffset toDate;
        if (!DateTimeOffset.TryParse(to, out toDate))
        {
            toDate = DateTimeOffset.UtcNow;
        } else
        {
            toDate = toDate.ToUniversalTime();
        }
        var repairOrders = await context.MaintenanceHistory
            .Where(m => m.CreatedAt >= fromDate && m.CreatedAt <= toDate)
            .ToListAsync(cancellationToken);
        var completedOrders = repairOrders.Where(r => r.TotalCost > 0).ToList();

        var serviceBookings = await context.ServiceBookings
            .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate)
            .ToListAsync(cancellationToken);
            
        var activeBookings = await context.ServiceBookings
            .Where(b => b.Status == Domain.Enums.BookingServiceStatus.InProgress.ToString() || 
                        b.Status == Domain.Enums.BookingServiceStatus.Pending.ToString() || 
                        b.Status == Domain.Enums.BookingServiceStatus.Confirmed.ToString())
            .ToListAsync(cancellationToken);
            
        var inProgressCount = activeBookings.Count(b => b.Status == Domain.Enums.BookingServiceStatus.InProgress.ToString());

        double avgHours = 0;
        var completedBookings = serviceBookings.Where(b => b.Status == Domain.Enums.BookingServiceStatus.Completed.ToString()).ToList();
        if (completedBookings.Any())
        {
            avgHours = completedBookings.Average(
                r => r.CompletedDate.HasValue && r.CreatedAt.HasValue
                    ? (r.CompletedDate.Value - r.CreatedAt.Value).TotalHours
                    : (r.UpdatedAt.HasValue && r.CreatedAt.HasValue ? (r.UpdatedAt.Value - r.CreatedAt.Value).TotalHours : 2.5));
        }
        var workshopPayments = await context.WorkshopPayments
            .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
            .ToListAsync(cancellationToken);
        var workshopRevenue = workshopPayments.Sum(p => p.TotalAmount);
        var retailOrders = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= fromDate &&
                    x.o.CreatedAt <= toDate &&
                    (x.o.StatusId == OrderStatus.Completed || x.o.StatusId == OrderStatus.Delivering))
            .ToListAsync(cancellationToken);
        var retailRevenue = retailOrders.Sum(x => (x.oi.Price ?? 0) * (x.oi.Count ?? 0));
        var empIds = completedOrders.Where(r => r.TechnicianId.HasValue)
            .Select(r => r.TechnicianId!.Value)
            .Distinct()
            .ToList();
        var employees = await context.EmployeeProfiles
            .Include(e => e.User)
            .Where(e => empIds.Contains(e.Id))
            .ToListAsync(cancellationToken);
        var techRankings = new List<TechnicianRankingDto>();
        foreach (var empId in empIds)
        {
            var empOrders = completedOrders.Where(r => r.TechnicianId == empId).ToList();
            var emp = employees.FirstOrDefault(e => e.Id == empId);
            techRankings.Add(
                new TechnicianRankingDto
                {
                    TechnicianName = emp?.User?.FullName ?? "Không rõ",
                    CompletedTickets = empOrders.Count,
                    TotalRevenue = empOrders.Sum(o => o.TotalCost),
                    ComplaintRate = 0
                });
        }
        techRankings = techRankings.OrderByDescending(t => t.CompletedTickets).ToList();
        var warrantyCount = await context.WarrantyClaims
            .Where(w => w.CreatedAt >= fromDate && w.CreatedAt <= toDate)
            .CountAsync(cancellationToken);
        var complaintsCount = await context.CustomerFeedbacks
            .Where(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate && c.FeedbackArea == "Workshop")
            .CountAsync(cancellationToken);
        var last6Months = Enumerable.Range(0, 6).Select(i => DateTimeOffset.UtcNow.AddMonths(-i)).Reverse().ToList();
        var revenueTrend = new RevenueTrendDto();
        foreach (var month in last6Months)
        {
            revenueTrend.Labels.Add(month.ToString("MM/yyyy"));
            var monthStart = new DateTimeOffset(month.Year, month.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
            var monthWorkshop = await context.WorkshopPayments
                .Where(p => p.CreatedAt >= monthStart && p.CreatedAt <= monthEnd)
                .SumAsync(p => p.TotalAmount, cancellationToken);
            revenueTrend.ServiceRevenue.Add(monthWorkshop);
            var monthRetail = await context.OutputInfos
                .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Where(
                    x => x.o.CreatedAt >= monthStart &&
                        x.o.CreatedAt <= monthEnd &&
                        (x.o.StatusId == OrderStatus.Completed || x.o.StatusId == OrderStatus.Delivering))
                .SumAsync(x => (x.oi.Price ?? 0) * (x.oi.Count ?? 0), cancellationToken);
            revenueTrend.RetailRevenue.Add(monthRetail);
        }
        var overdueCutoff = DateTimeOffset.UtcNow.AddHours(-48);
        var overdueBookings = activeBookings.Where(b => b.Status == Domain.Enums.BookingServiceStatus.InProgress.ToString() && (b.ScheduledDate < overdueCutoff || (b.CreatedAt.HasValue && b.CreatedAt.Value < overdueCutoff))).ToList();
        
        var overdueVehicleIds = overdueBookings.Where(b => b.VehicleId.HasValue).Select(b => b.VehicleId!.Value).Distinct().ToList();
        var vehicleOverdueDict = new Dictionary<int, string>();
        if (overdueVehicleIds.Any())
        {
            var vList = await context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Lead)
                .Where(v => overdueVehicleIds.Contains(v.Id))
                .Select(v => new { 
                    v.Id, 
                    CustomerName = v.User != null ? v.User.FullName : (v.Lead != null ? v.Lead.FullName : "-") 
                })
                .ToListAsync(cancellationToken);
            vehicleOverdueDict = vList.ToDictionary(x => x.Id, x => x.CustomerName);
        }
        
        var overdueTickets = overdueBookings.Select(
            b => new OverdueTicketDto
            {
                TicketId = b.Id,
                CustomerName = b.Customer != null ? b.Customer.FullName : (b.VehicleId.HasValue && vehicleOverdueDict.TryGetValue(b.VehicleId.Value, out var cn) ? cn : "-"),
                ExpectedCompletionTime = b.ScheduledDate.AddMinutes(b.EstimatedDurationMinutes ?? 60),
                Status = "Dang sua chua"
            })
            .ToList();
            
        var activeRepairOrders = await context.MaintenanceHistory
            .Where(m => m.TotalCost == 0)
            .ToListAsync(cancellationToken);

        var partShortages = new List<PartShortageDto>();
        foreach (var order in activeRepairOrders.Where(r => !string.IsNullOrEmpty(r.PartsJson)))
        {
            try
            {
                var partsData = JsonSerializer.Deserialize<PartsJsonDto>(order.PartsJson!);
                if (partsData?.Parts == null)
                    continue;
                foreach (var part in partsData.Parts)
                {
                    if (string.IsNullOrWhiteSpace(part.Name))
                        continue;
                    var availQty = await context.InventoryOnHands
                            .Where(
                                h => h.ProductVariant != null &&
                                        h.ProductVariant.UrlSlug != null &&
                                        EF.Functions
                                            .Like(h.ProductVariant.UrlSlug ?? string.Empty, "%" + part.Name + "%"))
                            .SumAsync(h => (int?)h.StockQty, cancellationToken) ??
                        0;
                    if (availQty < part.Qty)
                    {
                        partShortages.Add(
                            new PartShortageDto
                            {
                                TicketId = order.Id,
                                PartName = part.Name,
                                RequiredQuantity = part.Qty,
                                AvailableQuantity = availQty
                            });
                    }
                }
            } catch
            {
            }
        }
        var paymentMethodGroups = workshopPayments
    .GroupBy(p => string.IsNullOrEmpty(p.PaymentMethod) ? "Khac" : p.PaymentMethod)
            .Select(g => new RevenueSourceDto { Source = g.Key, Amount = g.Sum(p => p.TotalAmount) })
            .ToList();
        if (!paymentMethodGroups.Any())
        {
            paymentMethodGroups = new List<RevenueSourceDto> { new RevenueSourceDto { Source = "Khac", Amount = 0 } };
        }
        var repairOrderStatusCounts = new List<RepairOrderStatusCountDto>();
        var pendingStatusStr = Domain.Enums.BookingServiceStatus.Pending.ToString();
        var confirmedStatusStr = Domain.Enums.BookingServiceStatus.Confirmed.ToString();
        var inProgressStatusStr = Domain.Enums.BookingServiceStatus.InProgress.ToString();
        var completedStatusStr = Domain.Enums.BookingServiceStatus.Completed.ToString();
        var cancelledStatusStr = Domain.Enums.BookingServiceStatus.Cancelled.ToString();
        var noShowStatusStr = Domain.Enums.BookingServiceStatus.NoShow.ToString();
        
        int pendingBookingCount = serviceBookings.Count(b => b.Status == pendingStatusStr || b.Status == confirmedStatusStr);
        int inProgressBookingCount = serviceBookings.Count(b => b.Status == inProgressStatusStr);
        int completedBookingCount = serviceBookings.Count(b => b.Status == completedStatusStr);
        int cancelledBookingCount = serviceBookings.Count(b => b.Status == cancelledStatusStr || b.Status == noShowStatusStr);
        
        repairOrderStatusCounts.Add(new RepairOrderStatusCountDto { Status = "Cho sua chua", Count = pendingBookingCount });
        repairOrderStatusCounts.Add(new RepairOrderStatusCountDto { Status = "Dang sua chua", Count = inProgressBookingCount });
        repairOrderStatusCounts.Add(new RepairOrderStatusCountDto { Status = "Cho nghiem thu", Count = 0 });
        repairOrderStatusCounts.Add(new RepairOrderStatusCountDto { Status = "Da hoan thanh", Count = completedBookingCount });
        repairOrderStatusCounts.Add(new RepairOrderStatusCountDto { Status = "Da huy phieu", Count = cancelledBookingCount });
        return new WorkshopDashboardResponse
        {
            KpiCards =
                new KpiCards
                {
                    InProgressCount = inProgressCount,
                    AvgCompletionHours = Math.Round(avgHours, 1),
                    CumulativeRevenue = workshopRevenue
                },
            Alerts = new UrgentAlerts { OverdueTickets = overdueTickets, PartShortages = partShortages },
            Analytics =
                new Analytics
                {
                    RevenueComparison =
                        new WorkshopRevenueComparison
                            {
                                WorkshopRevenue = workshopRevenue,
                                RetailRevenue = retailRevenue
                            },
                    RevenueSources = paymentMethodGroups,
                    RevenueTrend = revenueTrend,
                    RepairOrderStatusCounts = repairOrderStatusCounts
                },
            Productivity =
                new Productivity
                {
                    TechnicianStatuses = new List<TechnicianStatusDto>(),
                    TechnicianRankings = techRankings
                },
            WarrantyRequestsCount = warrantyCount,
            ComplaintsCount = complaintsCount,
            RecentItems = new List<RecentItem>()
        };
    }

    private sealed record PartsJsonDto
    {
        public List<PartItemDto>? Parts { get; set; }
    }

    private sealed record PartItemDto
    {
        public string Name { get; set; } = string.Empty;

        public int Qty { get; set; }

        public decimal Price { get; set; }
    }

    public Task<List<RecentOrderResponse>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken)
    {
        return context.OutputOrders
            .IgnoreQueryFilters()
            .Where(o => string.Compare(o.StatusId, OrderStatus.Cancelled) != 0 && o.CreatedAt != null)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .Select(
                o => new RecentOrderResponse
                {
                    Id = o.Id,
                    OrderCode = $"HD{o.Id}",
                    BuyerName = o.CustomerName ?? (o.Buyer != null ? o.Buyer.FullName : "Khách lẻ"),
                    TotalAmount =
                        o.OutputInfos.Where(oi => oi.DeletedAt == null).Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)),
                    StatusId = o.StatusId,
                    CreatedAt = o.CreatedAt!.Value
                })
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TopProductRevenueResponse>> GetTopProductsByRevenueAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        int limit,
        CancellationToken cancellationToken)
    {
        var rawData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                    x.o.CreatedAt >= start &&
                    x.o.CreatedAt <= end)
            .Select(x => new { x.oi.ProductVariantId, Price = x.oi.Price ?? 0, Count = x.oi.Count ?? 0 })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var grouped = rawData.GroupBy(x => x.ProductVariantId)
            .Select(
                g => new { VariantId = g.Key, Revenue = g.Sum(x => x.Price * x.Count), SoldCount = g.Sum(x => x.Count) })
            .OrderByDescending(x => x.Revenue)
            .Take(limit)
            .ToList();
        var variantIds = grouped.Select(g => g.VariantId).ToList();
        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .Include(pv => pv.ProductVariantColors)
            .Where(pv => variantIds.Contains(pv.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return grouped.Select(
            g =>
            {
                var variant = variants.FirstOrDefault(v => v.Id == g.VariantId);
                return new TopProductRevenueResponse
                {
                    ProductName =
                        variant != null
                                ? $"{variant.Product?.Name} - {variant.VariantName} ({variant.ProductVariantColors.FirstOrDefault()?.ColorName?.Split(',').FirstOrDefault()})".Trim(
                                    ' ',
                                    '-',
                                    '(',
                                    ')')
                                : "Sản phẩm không xác định",
                    UnitsSold = g.SoldCount,
                    Revenue = g.Revenue
                };
            });
    }

    public async Task<IEnumerable<BrandRevenueResponse>> GetBrandRevenueDistributionAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var rawData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0)
            .Select(x => new { x.oi.ProductVariantId, Price = x.oi.Price ?? 0, Count = x.oi.Count ?? 0 })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variantIds = rawData.Select(x => x.ProductVariantId).Distinct().ToList();
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Include(pv => pv.Product)
            .ThenInclude(p => p!.Brand)
            .Where(pv => variantIds.Contains(pv.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var revenueData = rawData.Select(
            r => new
            {
                BrandName = variants.FirstOrDefault(v => v.Id == r.ProductVariantId)?.Product?.Brand?.Name ?? "Khác",
                Revenue = r.Price * r.Count
            });
        return[.. revenueData.GroupBy(r => r.BrandName)
            .Select(g => new BrandRevenueResponse { BrandName = g.Key, Revenue = g.Sum(x => x.Revenue) })
            .OrderByDescending(b => b.Revenue)];
    }

    public async Task<IEnumerable<DailyRevenueTableResponse>> GetDailyRevenueTableDataAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var days = (int)(end - start).TotalDays + 1;
        if (days <= 0)
            days = 1;
        var startDate = DateOnly.FromDateTime(start.Date);
        var startDateTimeOffset = start;
        var endDateTimeOffset = end;
        var dateSeries = Enumerable.Range(0, days).Select(i => startDate.AddDays(i)).ToList();
        var rawData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                    x.o.CreatedAt != null &&
                    x.o.CreatedAt >= startDateTimeOffset &&
                    x.o.CreatedAt <= endDateTimeOffset)
            .Select(
                x => new
                {
                    CreatedAt = x.o.CreatedAt!.Value,
                    OrderId = x.o.Id,
                    Price = x.oi.Price ?? 0,
                    CostPrice = x.oi.CostPrice ?? 0,
                    Count = x.oi.Count ?? 0
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var revenueData = rawData
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAt.DateTime))
            .Select(
                g => new
                {
                    Day = g.Key,
                    OrdersCount = g.Select(x => x.OrderId).Distinct().Count(),
                    Revenue = g.Sum(x => x.Price * x.Count),
                    Profit = g.Sum(x => (x.Price - x.CostPrice) * x.Count),
                    HasZeroCostPrice = g.Any(x => x.CostPrice == 0)
                })
            .ToList();
        var result = new List<DailyRevenueTableResponse>();
        for (int i = 0; i < dateSeries.Count; i++)
        {
            var day = dateSeries[i];
            var dayData = revenueData.FirstOrDefault(r => r.Day == day);
            var prevDayData = i > 0 ? revenueData.FirstOrDefault(r => r.Day == dateSeries[i - 1]) : null;
            double growth = 0;
            if (prevDayData != null && prevDayData.Revenue > 0 && dayData != null)
            {
                growth = (double)((dayData.Revenue - prevDayData.Revenue) / prevDayData.Revenue * 100);
            }
            result.Add(
                new DailyRevenueTableResponse
                {
                    ReportDay = day,
                    OrdersCount = dayData?.OrdersCount ?? 0,
                    TotalRevenue = dayData?.Revenue ?? 0,
                    TotalProfit = dayData?.Profit ?? 0,
                    Growth = Math.Round(growth, 2),
                    HasZeroCostPrice = dayData?.HasZeroCostPrice ?? false
                });
        }
        return result.OrderByDescending(r => r.ReportDay);
    }

    public async Task<IEnumerable<DailyRevenueDetailResponse>> GetDailyRevenueDetailAsync(
        DateOnly reportDay,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var dayStart = new DateTimeOffset(reportDay.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1).AddTicks(-1);
        var rawData = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= dayStart &&
                    x.o.CreatedAt <= dayEnd &&
                    (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .Select(x => new { x.oi.ProductVariantId, x.o.CreatedBy, Price = x.oi.Price ?? 0, Count = x.oi.Count ?? 0 })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variantIds = rawData.Select(x => x.ProductVariantId).Distinct().ToList();
        var userIds = rawData.Select(x => x.CreatedBy)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Include(pv => pv.Product)
            .Where(pv => variantIds.Contains(pv.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var users = await context.Users
            .IgnoreQueryFilters()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rawData
        .GroupBy(x => new { x.ProductVariantId, CreatedBy = x.CreatedBy ?? Guid.Empty })
            .Select(
                g =>
                {
                    var variant = variants.FirstOrDefault(v => v.Id == g.Key.ProductVariantId);
                    var user = users.FirstOrDefault(u => u.Id == g.Key.CreatedBy);
                    return new DailyRevenueDetailResponse
                    {
                        ProductName =
                            variant != null
                                    ? $"{variant.Product?.Name} - {variant.VariantName}".Trim(' ', '-')
                                    : "Sản phẩm không xác định",
                        EmployeeName = user?.FullName ?? "Không rõ",
                        Quantity = g.Sum(x => x.Count),
                        Revenue = g.Sum(x => x.Price * x.Count)
                    };
                })
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }

    public async Task<IEnumerable<DailyRevenueResponse>> GetDailyRevenueAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var endDateTimeOffset = end.Hour == 0 && end.Minute == 0 && end.Second == 0
            ? end.Date.AddDays(1).AddTicks(-1)
            : end;
        var startDateTimeOffset = start;
        var vnStart = startDateTimeOffset.ToOffset(TimeSpan.FromHours(7)).Date;
        var vnEnd = endDateTimeOffset.ToOffset(TimeSpan.FromHours(7)).Date;
        var days = (int)(vnEnd - vnStart).TotalDays + 1;
        if (days <= 0)
            days = 1;
        var startDate = DateOnly.FromDateTime(vnStart);
        var dateSeries = Enumerable.Range(0, days).Select(i => startDate.AddDays(i)).ToList();
        var rawData = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                    x.o.CreatedAt != null &&
                    x.o.CreatedAt >= startDateTimeOffset &&
                    x.o.CreatedAt <= endDateTimeOffset)
            .Select(x => new { CreatedAt = x.o.CreatedAt!.Value, Price = x.oi.Price ?? 0, Count = x.oi.Count ?? 0 })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var revenueData = rawData
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAt.ToOffset(TimeSpan.FromHours(7)).DateTime))
            .Select(g => new { Day = g.Key, Revenue = g.Sum(x => x.Price * x.Count) })
            .ToList();
        return dateSeries.Select(
            day => new DailyRevenueResponse
            {
                ReportDay = day,
                TotalRevenue = revenueData.FirstOrDefault(r => r.Day == day)?.Revenue ?? 0
            });
    }

    public async Task<DashboardStatsResponse?> GetDashboardStatsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var vnTz = TimeSpan.FromHours(7);
        var nowVn = now.ToOffset(vnTz);

        // Chuẩn hóa mốc ngày Dương lịch theo múi giờ VN (GMT+7)
        var todayStart = new DateTimeOffset(nowVn.Year, nowVn.Month, nowVn.Day, 0, 0, 0, vnTz);
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);
        var yesterdayStart = todayStart.AddDays(-1);
        var yesterdayEnd = todayStart.AddTicks(-1);

        // Tháng Dương lịch hiện tại (01/MM/yyyy -> cuối tháng)
        var currentMonthStart = new DateTimeOffset(nowVn.Year, nowVn.Month, 1, 0, 0, 0, vnTz);
        var currentMonthEnd = currentMonthStart.AddMonths(1).AddTicks(-1);
        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastMonthEnd = currentMonthStart.AddTicks(-1);

        // Năm Dương lịch hiện tại (01/01/yyyy -> 31/12/yyyy)
        var currentYearStart = new DateTimeOffset(nowVn.Year, 1, 1, 0, 0, 0, vnTz);
        var currentYearEnd = currentYearStart.AddYears(1).AddTicks(-1);
        var lastYearStart = currentYearStart.AddYears(-1);
        var lastYearEnd = currentYearStart.AddTicks(-1);

        // Chuẩn hóa khoảng thời gian do người dùng chọn (start, end)
        var periodStart = start;
        var periodEnd = end.Hour == 0 && end.Minute == 0 && end.Second == 0
            ? end.Date.AddDays(1).AddTicks(-1)
            : end;

        var revenueQueryStart = periodStart < lastYearStart ? periodStart : lastYearStart;
        var revenueQueryEnd = periodEnd > currentYearEnd ? periodEnd : currentYearEnd;
        var revenueRows = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt != null &&
                    x.o.CreatedAt >= revenueQueryStart &&
                    x.o.CreatedAt <= revenueQueryEnd &&
                    (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .GroupBy(x => new
            {
                x.o.CreatedAt!.Value.Year,
                x.o.CreatedAt.Value.Month,
                x.o.CreatedAt.Value.Day
            })
            .Select(
                g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    Revenue = g.Sum(x => (x.oi.Price ?? 0) * (x.oi.Count ?? 0)),
                    Profit = g.Sum(x => ((x.oi.Price ?? 0) - (x.oi.CostPrice ?? 0)) * (x.oi.Count ?? 0))
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var revenueByDay = revenueRows
            .Select(x => new
            {
                Date = new DateOnly(x.Year, x.Month, x.Day),
                x.Revenue,
                x.Profit
            })
            .ToList();

        (decimal Rev, decimal Prof) GetStatsInRange(DateTimeOffset rangeStart, DateTimeOffset rangeEnd)
        {
            var startDay = DateOnly.FromDateTime(rangeStart.ToOffset(vnTz).DateTime);
            var endDay = DateOnly.FromDateTime(rangeEnd.ToOffset(vnTz).DateTime);
            var rows = revenueByDay.Where(x => x.Date >= startDay && x.Date <= endDay);
            return (rows.Sum(x => x.Revenue), rows.Sum(x => x.Profit));
        }

        var (todayRev, todayProf) = GetStatsInRange(todayStart, todayEnd);
        var (yesterdayRev, _) = GetStatsInRange(yesterdayStart, yesterdayEnd);
        var (monthRev, monthProf) = GetStatsInRange(currentMonthStart, currentMonthEnd);
        var (lastMonthRev, lastMonthProf) = GetStatsInRange(lastMonthStart, lastMonthEnd);
        var (yearRev, yearProf) = GetStatsInRange(currentYearStart, currentYearEnd);
        var (lastYearRev, lastYearProf) = GetStatsInRange(lastYearStart, lastYearEnd);
        var (periodRev, periodProf) = GetStatsInRange(periodStart, periodEnd);

        decimal revenueChange = 0;
        if (yesterdayRev > 0)
            revenueChange = ((todayRev - yesterdayRev) / yesterdayRev) * 100;
        else if (todayRev > 0)
            revenueChange = 100;

        var twoHoursAgo = now.AddHours(-2);
        var overdueOrdersCount = await context.OutputOrders
            .IgnoreQueryFilters()
            .CountAsync(
                o => (string.Compare(o.StatusId, OrderStatus.Pending) == 0 ||
                        string.Compare(o.StatusId, OrderStatus.WaitingDeposit) == 0) &&
                    o.CreatedAt != null &&
                    o.CreatedAt <= twoHoursAgo,
                cancellationToken)
            .ConfigureAwait(false);

        var thirtyDaysAgo = now.AddDays(-30);
        var pendingOrdersCount = await context.OutputOrders
            .IgnoreQueryFilters()
            .CountAsync(
                o => (string.Compare(o.StatusId, OrderStatus.Pending) == 0 ||
                        string.Compare(o.StatusId, OrderStatus.WaitingDeposit) == 0) &&
                    o.CreatedAt != null &&
                    o.CreatedAt >= thirtyDaysAgo,
                cancellationToken)
            .ConfigureAwait(false);

        async Task<int> GetVehiclesSold(DateTimeOffset rangeStart, DateTimeOffset rangeEnd)
        {
            return await context.OutputInfos
                .IgnoreQueryFilters()
                .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Join(
                    context.ProductVariants.IgnoreQueryFilters(),
                    x => x.oi.ProductVariantId,
                    pv => pv.Id,
                    (x, pv) => new { x.oi, x.o, pv })
                .Join(
                    context.Products.IgnoreQueryFilters(),
                    x => x.pv.ProductId,
                    p => p.Id,
                    (x, p) => new { x.oi, x.o, p })
                .Join(
                    context.ProductCategories.IgnoreQueryFilters(),
                    x => x.p.CategoryId,
                    c => c.Id,
                    (x, c) => new { x.oi, x.o, c })
                .Where(
                    x => x.o.CreatedAt >= rangeStart &&
                        x.o.CreatedAt <= rangeEnd &&
                        (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                            string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                            string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                        ((x.c.Slug != null && x.c.Slug.Contains("xe")) ||
                            (x.c.Name != null && (x.c.Name.Contains("Xe") || x.c.Name.Contains("xe")))))
                .SumAsync(x => x.oi.Count ?? 0, cancellationToken)
                .ConfigureAwait(false);
        }

        var todayVehicles = await GetVehiclesSold(todayStart, todayEnd).ConfigureAwait(false);
        var monthVehicles = await GetVehiclesSold(currentMonthStart, currentMonthEnd).ConfigureAwait(false);

        // Tồn kho: Đồng bộ với Phân hệ Kho (Warehouse)
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Where(pv => pv.DeletedAt == null)
            .Select(
                pv => new
                {
                    pv.Id,
                    pv.ProductId,
                    BrandName = pv.Product != null && pv.Product.Brand != null ? pv.Product.Brand.Name : "Khác"
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var confirmedInboundRaw = await context.InventoryReceiptInfos
            .IgnoreQueryFilters()
            .Join(
                context.InventoryReceipts.IgnoreQueryFilters(),
                ii => ii.InventoryReceiptId,
                i => i.Id,
                (ii, i) => new { ii, i })
            .Where(
                x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0 &&
                    x.ii.DeletedAt == null &&
                    x.i.DeletedAt == null)
            .Where(x => x.ii.PurchaseRequestItem != null)
            .GroupBy(x => x.ii.PurchaseRequestItem!.ProductVariantId)
            .Select(x => new { VariantId = x.Key, TotalIn = x.Sum(y => (long)(y.ii.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var soldOutputsAll = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.oi.ProductVariantId != null &&
                    (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .GroupBy(x => x.oi.ProductVariantId)
            .Select(g => new { VariantId = g.Key!.Value, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var confirmedInbound = confirmedInboundRaw.ToDictionary(x => x.VariantId, x => x.TotalIn);
        var soldOutputs = soldOutputsAll.ToDictionary(x => x.VariantId, x => x.TotalOut);
        var variantStockList = variants.Select(pv =>
        {
            var tin = confirmedInbound.GetValueOrDefault(pv.Id);
            var tout = soldOutputs.GetValueOrDefault(pv.Id);
            var stock = Math.Max(0, (int)(tin - tout));
            return new
            {
                VariantId = pv.Id,
                ProductId = pv.ProductId,
                BrandName = pv.BrandName,
                Stock = stock
            };
        }).ToList();

        var productStockList = variantStockList
            .GroupBy(x => new { x.ProductId, x.BrandName })
            .Select(g => new { g.Key.ProductId, g.Key.BrandName, Stock = g.Sum(x => x.Stock) })
            .ToList();
        var totalInventory = productStockList.Sum(x => x.Stock);
        var lowStockCount = productStockList.Count(x => x.Stock > 0 && x.Stock < 5);
        var outOfStockCount = productStockList.Count(x => x.Stock == 0);

        var brandStock = productStockList
            .GroupBy(x => x.BrandName)
            .Select(g => new BrandStockResponse { BrandName = g.Key, StockCount = g.Sum(x => x.Stock) })
            .Where(x => x.StockCount > 0)
            .OrderByDescending(x => x.StockCount)
            .Take(5)
            .ToList();

        var last7DaysStart = todayStart.AddDays(-6);
        var last7DaysData = revenueByDay
            .Where(x => x.Date >= DateOnly.FromDateTime(last7DaysStart.ToOffset(vnTz).DateTime))
            .Select(x => new { Date = x.Date, x.Revenue, x.Profit })
            .ToList();
        decimal total7dRev = last7DaysData.Sum(x => x.Revenue);
        decimal total7dProf = last7DaysData.Sum(x => x.Profit);
        var bestDay = last7DaysData.OrderByDescending(x => x.Revenue).FirstOrDefault();
        var totalProducts = variants.Select(x => x.ProductId).Distinct().Count();
        var activeInstallments = await context.OutputOrders
            .IgnoreQueryFilters()
            .CountAsync(
                o => string.Compare(o.StatusId, OrderStatus.WaitingDeposit) == 0 ||
                    string.Compare(o.StatusId, OrderStatus.DepositPaid) == 0,
                cancellationToken)
            .ConfigureAwait(false);

        // Doanh thu theo thương hiệu & Top sản phẩm: sử dụng periodStart & periodEnd linh hoạt (nếu không có thì mặc định 7 ngày gần nhất)
        var brandRevenueStart = periodStart;
        var brandRevenueEnd = periodEnd;
        if (start == default || end == default)
        {
            brandRevenueStart = last7DaysStart;
            brandRevenueEnd = todayEnd;
        }

        var topProducts = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= brandRevenueStart &&
                    x.o.CreatedAt <= brandRevenueEnd &&
                    (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .Join(context.ProductVariants, x => x.oi.ProductVariantId, pv => pv.Id, (x, pv) => new { x.oi, x.o, pv })
            .Join(context.Products, x => x.pv.ProductId, p => p.Id, (x, p) => new { x.oi, x.o, p })
            .GroupBy(x => x.p.Name)
            .Select(
                g => new TopSellingProductResponse
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.oi.Count ?? 0),
                    TotalRevenue = g.Sum(x => (x.oi.Price ?? 0) * (x.oi.Count ?? 0))
                })
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var brandRevenue = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= brandRevenueStart &&
                    x.o.CreatedAt <= brandRevenueEnd &&
                    (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .Join(
                context.ProductVariants.IgnoreQueryFilters(),
                x => x.oi.ProductVariantId,
                pv => pv.Id,
                (x, pv) => new { x.oi, x.o, pv })
            .Join(context.Products.IgnoreQueryFilters(), x => x.pv.ProductId, p => p.Id, (x, p) => new { x.oi, x.o, p })
            .Join(context.Brands.IgnoreQueryFilters(), x => x.p.BrandId, b => b.Id, (x, b) => new { x.oi, x.o, b })
            .GroupBy(x => x.b.Name)
            .Select(
                g => new BrandRevenueResponse
                {
                    BrandName = g.Key,
                    Revenue = g.Sum(x => (x.oi.Price ?? 0) * (x.oi.Count ?? 0)),
                    TotalRevenue = g.Sum(x => (x.oi.Price ?? 0) * (x.oi.Count ?? 0)),
                    QuantitySold = g.Sum(x => x.oi.Count ?? 0)
                })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var todayActivities = new List<string>();
        if (todayVehicles > 0)
            todayActivities.Add($"{todayVehicles} xe đã giao");
        var todayInst = await context.OutputOrders
            .IgnoreQueryFilters()
            .CountAsync(
                o => o.CreatedAt >= todayStart &&
                    (string.Compare(o.StatusId, OrderStatus.WaitingDeposit) == 0 ||
                        string.Compare(o.StatusId, OrderStatus.DepositPaid) == 0),
                cancellationToken)
            .ConfigureAwait(false);
        if (todayInst > 0)
            todayActivities.Add($"{todayInst} đơn trả góp mới");
        var todayCust = await context.Users
            .IgnoreQueryFilters()
            .CountAsync(u => u.CreatedAt >= todayStart, cancellationToken)
            .ConfigureAwait(false);
        if (todayCust > 0)
            todayActivities.Add($"{todayCust} khách ghé thăm");

        return new DashboardStatsResponse
        {
            TodayRevenue = todayRev,
            RevenueChangePercentage = revenueChange,
            PeriodRevenue = periodRev,
            PeriodProfit = periodProf,
            MonthlyRevenue = monthRev,
            TodayProfit = todayProf,
            MonthlyProfit = monthProf,
            LastMonthRevenue = lastMonthRev,
            LastMonthProfit = lastMonthProf,
            YearlyRevenue = yearRev,
            YearlyProfit = yearProf,
            LastYearRevenue = lastYearRev,
            LastYearProfit = lastYearProf,
            Total7dRevenue = total7dRev,
            Total7dProfit = total7dProf,
            BestDayRevenue = bestDay?.Revenue ?? 0,
            BestDayDate = bestDay != null ? $"{bestDay.Date.Day:D2}/{bestDay.Date.Month:D2}" : null,
            OverdueOrdersCount = overdueOrdersCount,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount,
            TodayVehiclesSold = todayVehicles,
            MonthlyVehiclesSold = monthVehicles,
            CurrentInventoryCount = totalInventory,
            TotalSKUCount = totalProducts,
            OverstockCount = 0,
            BrandDistribution = brandStock,
            ActiveInstallmentCount = activeInstallments,
            LateInstallmentCount = (int)(activeInstallments * 0.1),
            TotalCustomerDebt = 0,
            OverdueDebtAmount = 0,
            PendingOrdersCount = pendingOrdersCount,
            NewCustomersCount = todayCust,
            TopSellingProducts = topProducts,
            BrandRevenueDistribution = brandRevenue,
            TodayActivities = todayActivities
        };
    }

    public async Task<IEnumerable<MonthlyRevenueProfitResponse>> GetMonthlyRevenueProfitAsync(
        int months,
        CancellationToken cancellationToken)
    {
        var vietnamOffset = TimeSpan.FromHours(7);
        var now = DateTimeOffset.UtcNow.ToOffset(vietnamOffset);
        var currentMonth = new DateOnly(now.Year, now.Month, 1);
        var startMonth = currentMonth.AddMonths(-(months - 1));
        var startDateTimeOffset = new DateTimeOffset(
            startMonth.ToDateTime(TimeOnly.MinValue),
            vietnamOffset);
        var monthSeries = Enumerable.Range(0, months).Select(i => startMonth.AddMonths(i)).ToList();
        var rawData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                    x.o.CreatedAt != null &&
                    x.o.CreatedAt >= startDateTimeOffset)
            .Select(
                x => new
                {
                    CreatedAt = x.o.CreatedAt!.Value,
                    Price = x.oi.Price ?? 0,
                    CostPrice = x.oi.CostPrice ?? 0,
                    Count = x.oi.Count ?? 0
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var orderRevenueData = rawData
            .GroupBy(
                x =>
                {
                    var localDate = x.CreatedAt.ToOffset(vietnamOffset);
                    return new DateOnly(localDate.Year, localDate.Month, 1);
                })
            .Select(
                g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => x.Price * x.Count),
                    Profit = g.Sum(x => (x.Price - x.CostPrice) * x.Count),
                    HasZeroCostPrice = g.Any(x => x.CostPrice == 0)
                })
            .ToList();

        var invoiceQueryStart = startMonth.ToDateTime(TimeOnly.MinValue).AddDays(-1);
        var invoiceRows = await context.Invoices
            .Where(
                i => string.Compare(i.Status, OrderStatus.Completed) == 0 &&
                    (i.ProcessedAt ?? i.IssueDate) >= invoiceQueryStart)
            .Select(
                i => new
                {
                    i.IssueDate,
                    i.ProcessedAt,
                    i.TotalAmount,
                    i.VehiclePrice,
                    i.ChassisNo,
                    i.EngineNo
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var chassisNumbers = invoiceRows
            .Select(i => i.ChassisNo)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .ToList();
        var engineNumbers = invoiceRows
            .Select(i => i.EngineNo)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .ToList();
        var invoiceVehicles = await context.Vehicles
            .Where(v => chassisNumbers.Contains(v.VinNumber) || engineNumbers.Contains(v.EngineNumber))
            .Select(v => new { v.VinNumber, v.EngineNumber, v.ImportPrice })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var importPriceByIdentifier = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var vehicle in invoiceVehicles)
        {
            if (!string.IsNullOrWhiteSpace(vehicle.VinNumber))
                importPriceByIdentifier.TryAdd(vehicle.VinNumber, vehicle.ImportPrice);
            if (!string.IsNullOrWhiteSpace(vehicle.EngineNumber))
                importPriceByIdentifier.TryAdd(vehicle.EngineNumber, vehicle.ImportPrice);
        }
        var invoiceRevenueData = invoiceRows
            .Select(
                invoice =>
                {
                    var recognizedAt = invoice.ProcessedAt ?? invoice.IssueDate;
                    if (recognizedAt.Kind == DateTimeKind.Utc)
                    {
                        recognizedAt = new DateTimeOffset(recognizedAt, TimeSpan.Zero)
                            .ToOffset(vietnamOffset)
                            .DateTime;
                    }
                    var month = new DateOnly(recognizedAt.Year, recognizedAt.Month, 1);
                    var importPrice = 0m;
                    if (!string.IsNullOrWhiteSpace(invoice.ChassisNo))
                        importPriceByIdentifier.TryGetValue(invoice.ChassisNo, out importPrice);
                    if (importPrice <= 0 && !string.IsNullOrWhiteSpace(invoice.EngineNo))
                        importPriceByIdentifier.TryGetValue(invoice.EngineNo, out importPrice);
                    return new
                    {
                        Month = month,
                        Revenue = invoice.TotalAmount,
                        Profit = importPrice > 0 ? Math.Max(0, invoice.VehiclePrice - importPrice) : 0,
                        HasZeroCostPrice = invoice.VehiclePrice > 0 && importPrice <= 0
                    };
                })
            .Where(row => row.Month >= startMonth)
            .GroupBy(row => row.Month)
            .Select(
                g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => x.Revenue),
                    Profit = g.Sum(x => x.Profit),
                    HasZeroCostPrice = g.Any(x => x.HasZeroCostPrice)
                })
            .ToList();
        var revenueData = orderRevenueData
            .Concat(invoiceRevenueData)
            .GroupBy(row => row.Month)
            .Select(
                g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => x.Revenue),
                    Profit = g.Sum(x => x.Profit),
                    HasZeroCostPrice = g.Any(x => x.HasZeroCostPrice)
                })
            .ToList();
        return monthSeries.Select(
            month => new MonthlyRevenueProfitResponse
            {
                ReportMonth = month,
                TotalRevenue = revenueData.FirstOrDefault(r => r.Month == month)?.Revenue ?? 0,
                TotalProfit = revenueData.FirstOrDefault(r => r.Month == month)?.Profit ?? 0,
                HasZeroCostPrice = revenueData.FirstOrDefault(r => r.Month == month)?.HasZeroCostPrice ?? false
            });
    }

    public Task<IEnumerable<OrderStatusCountResponse>> GetOrderStatusCountsAsync(CancellationToken cancellationToken)
    {
        return context.OutputStatuses
            .IgnoreQueryFilters()
            .GroupJoin(
                context.OutputOrders.IgnoreQueryFilters(),
                os => os.Key,
                o => o.StatusId,
                (os, orders) => new OrderStatusCountResponse { StatusName = os.Key, OrderCount = orders.Count() })
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<OrderStatusCountResponse>>(t => t.Result, cancellationToken);
    }

    public async Task<IEnumerable<ProductReportResponse>> GetProductReportLastMonthAsync(
        CancellationToken cancellationToken)
    {
        var lastMonthStart = new DateTimeOffset(
            DateTimeOffset.UtcNow.AddMonths(-1).Year,
            DateTimeOffset.UtcNow.AddMonths(-1).Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);
        var currentMonthStart = new DateTimeOffset(
            DateTimeOffset.UtcNow.Year,
            DateTimeOffset.UtcNow.Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);
        var confirmedInventoryReceiptsRaw = await context.InventoryReceiptInfos
            .IgnoreQueryFilters()
            .Join(
                context.InventoryReceipts.IgnoreQueryFilters(),
                ii => ii.InventoryReceiptId,
                i => i.Id,
                (ii, i) => new { ii, i })
            .Where(
                x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0 &&
                    x.ii.PurchaseRequestItem != null)
            .Select(
                x => new { VariantId = x.ii.PurchaseRequestItem!.ProductVariantId, Count = (long)(x.ii.Count ?? 0) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var confirmedInventoryReceipts = confirmedInventoryReceiptsRaw
            .GroupBy(x => x.VariantId)
            .Select(g => new { VariantId = (int?)g.Key, TotalIn = g.Sum(x => x.Count) })
            .ToList();
        var soldOutputsAll = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0)
            .GroupBy(x => x.oi.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldLastMonth = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                    x.o.CreatedAt >= lastMonthStart &&
                    x.o.CreatedAt < currentMonthStart)
            .GroupBy(x => x.oi.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalSold = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Include(pv => pv.Product)
            .Include(pv => pv.ProductVariantColors)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return variants.Select(
            pv => new ProductReportResponse
            {
                ProductName =
                    $"{pv.Product?.Name} - {pv.VariantName} ({pv.ProductVariantColors.FirstOrDefault()?.ColorName?.Split(',').FirstOrDefault()})".Trim(
                            ' ',
                            '-',
                            '(',
                            ')'),
                VariantId = pv.Id,
                StockQuantity =
                    (int)((confirmedInventoryReceipts.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalIn ?? 0) -
                            (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0)),
                SoldLastMonth = (int)(soldLastMonth.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalSold ?? 0)
            });
    }

    public async Task<IEnumerable<ProductPerformanceTableResponse>> GetProductPerformanceTableAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var last30Days = new DateTimeOffset(DateTime.UtcNow.AddDays(-30), TimeSpan.Zero);
        var confirmedInventoryReceiptsRaw = await context.InventoryReceiptInfos
            .Join(context.InventoryReceipts, ii => ii.InventoryReceiptId, i => i.Id, (ii, i) => new { ii, i })
            .Where(
                x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0 &&
                    x.ii.DeletedAt == null &&
                    x.i.DeletedAt == null &&
                    x.ii.PurchaseRequestItem != null)
            .Select(
                x => new { VariantId = x.ii.PurchaseRequestItem!.ProductVariantId, Count = (long)(x.ii.Count ?? 0) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var confirmedInventoryReceipts = confirmedInventoryReceiptsRaw
            .GroupBy(x => x.VariantId)
            .Select(g => new { VariantId = (int?)g.Key, TotalIn = g.Sum(x => x.Count) })
            .ToList();
        var outputsData = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0)
            .Select(
                x => new
                {
                    x.oi.ProductVariantId,
                    x.o.CreatedAt,
                    Count = x.oi.Count ?? 0,
                    Price = x.oi.Price ?? 0,
                    Cost = x.oi.CostPrice ?? 0
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldOutputsAll = outputsData
            .GroupBy(x => x.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)x.Count) })
            .ToList();
        var soldLast30Days = outputsData
            .Where(x => x.CreatedAt >= last30Days)
            .GroupBy(x => x.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalSold = g.Sum(x => (long)x.Count) })
            .ToList();
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Include(pv => pv.Product)
            .Include(pv => pv.ProductVariantColors)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return[.. variants.Select(
            pv =>
            {
                var stock = (int)((confirmedInventoryReceipts.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalIn ?? 0) -
                    (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0));
                var sold30 = (int)(soldLast30Days.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalSold ?? 0);
                var variantOutputs = outputsData.Where(x => x.ProductVariantId == pv.Id).ToList();
                var totalRevenue = variantOutputs.Sum(x => x.Price * x.Count);
                var totalCost = variantOutputs.Sum(x => x.Cost * x.Count);
                var margin = totalRevenue > 0 ? (double)((totalRevenue - totalCost) / totalRevenue * 100) : 0;
                var sellPrice = pv.Price ?? 0;
                return new ProductPerformanceTableResponse
                {
                    ProductName =
                        $"{pv.Product?.Name} - {pv.VariantName} ({pv.ProductVariantColors.FirstOrDefault()?.ColorName?.Split(',').FirstOrDefault()})".Trim(
                                ' ',
                                '-',
                                '(',
                                ')'),
                    SellPrice = sellPrice,
                    SoldCount30Days = sold30,
                    StockQuantity = stock,
                    MaxStockQuantity = 100,
                    MarginPercentage = Math.Round(margin, 1),
                    Status = stock <= 0 ? "Hết hàng" : (stock < 5 ? "Sắp hết" : "Còn hàng"),
                    Trend = [0, 0, sold30]
                };
            })];
    }

    public async Task<IEnumerable<WarehouseTableDataResponse>> GetWarehouseTableDataAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Where(pv => pv.DeletedAt == null)
            .Include(pv => pv.Product)
            .ThenInclude(p => p!.Brand)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var confirmedInventoryReceiptsRaw = await context.InventoryReceiptInfos
            .IgnoreQueryFilters()
            .Join(
                context.InventoryReceipts.IgnoreQueryFilters(),
                ii => ii.InventoryReceiptId,
                i => i.Id,
                (ii, i) => new { ii, i })
            .Where(
                x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0 &&
                    x.ii.DeletedAt == null &&
                    x.i.DeletedAt == null &&
                    x.ii.PurchaseRequestItem != null)
            .Select(
                x => new
                {
                    VariantId = x.ii.PurchaseRequestItem!.ProductVariantId,
                    Count = (long)(x.ii.Count ?? 0),
                    Cost = (x.ii.PurchaseRequestItem.UnitPrice ?? 0m) * (x.ii.Count ?? 0)
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var confirmedInventoryReceipts = confirmedInventoryReceiptsRaw
            .GroupBy(x => x.VariantId)
            .Select(g => new { VariantId = (int?)g.Key, TotalIn = g.Sum(x => x.Count), TotalCost = g.Sum(x => x.Cost) })
            .ToList();
        var soldOutputsAll = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0)
            .GroupBy(x => x.oi.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variantDatas = variants.Select(
            pv =>
            {
                var inboundData = confirmedInventoryReceipts.FirstOrDefault(x => x.VariantId == pv.Id);
                var totalIn = inboundData?.TotalIn ?? 0;
                var totalOut = soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0;
                var stock = Math.Max(0, (int)(totalIn - totalOut));
                var avgCostPrice = (totalIn > 0 && inboundData != null)
                    ? (inboundData.TotalCost / (decimal)inboundData.TotalIn)
                    : 0m;
                return new { BrandName = pv.Product?.Brand?.Name, Stock = stock, Value = stock * avgCostPrice };
            });
        var grouped = variantDatas
            .GroupBy(x => x.BrandName ?? "Khác")
            .Select(
                g =>
                {
                    int totalStock = g.Sum(x => x.Stock);
                    int lowStock = g.Count(x => x.Stock > 0 && x.Stock < 5);
                    int outOfStock = g.Count(x => x.Stock == 0);
                    decimal value = g.Sum(x => x.Value);
                    int capacity = g.Count();
                    string status = outOfStock > 0 ? "Cảnh báo" : (lowStock > 0 ? "Sắp hết" : "Bình thường");
                    return new WarehouseTableDataResponse
                    {
                        BrandName = g.Key,
                        TotalStock = totalStock,
                        Capacity = capacity,
                        LowStock = lowStock,
                        OutOfStock = outOfStock,
                        Status = status,
                        Value = value
                    };
                })
            .OrderByDescending(x => x.TotalStock)
            .ToList();
        return grouped;
    }

    public async Task<ProductStockPriceResponse?> GetProductStockAndPriceAsync(
        int variantId,
        CancellationToken cancellationToken)
    {
        var variant = await context.ProductVariants
            .FirstOrDefaultAsync(pv => pv.Id == variantId, cancellationToken)
            .ConfigureAwait(false);
        if (variant is null)
        {
            return null;
        }
        var totalInventoryReceipt = await context.InventoryReceiptInfos
                .IgnoreQueryFilters()
                .Join(
                    context.InventoryReceipts.IgnoreQueryFilters(),
                    ii => ii.InventoryReceiptId,
                    i => i.Id,
                    (ii, i) => new { ii, i })
                .Where(
                    x => ((x.ii.PurchaseRequestItem != null ? x.ii.PurchaseRequestItem.ProductVariantId : (int?)null)) ==
                            variantId &&
                            string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0)
                .SumAsync(x => (long?)(x.ii.Count ?? 0), cancellationToken)
                .ConfigureAwait(false) ??
            0;
        var totalOutput = await context.OutputInfos
                .IgnoreQueryFilters()
                .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Where(
                    x => x.oi.ProductVariantId == variantId &&
                            (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                                string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                                string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
                .SumAsync(x => (long?)(x.oi.Count ?? 0), cancellationToken)
                .ConfigureAwait(false) ??
            0;
        return new ProductStockPriceResponse
        {
            UnitPrice = variant.Price ?? 0,
            StockQuantity = (int)totalInventoryReceipt - (int)totalOutput
        };
    }

    private async Task<List<ConfirmedInputSummary>> GetConfirmedInputSummariesAsync(
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        if (string.Compare(context.Database.ProviderName, "Microsoft.EntityFrameworkCore.SqlServer") != 0)
        {
            var query = includeDeleted
                ? context.InventoryReceiptInfos
                    .IgnoreQueryFilters()
                    .Join(
                        context.InventoryReceipts.IgnoreQueryFilters(),
                        ii => ii.InventoryReceiptId,
                        i => i.Id,
                        (ii, i) => new { ii, i })
                : context.InventoryReceiptInfos
                    .Join(context.InventoryReceipts, ii => ii.InventoryReceiptId, i => i.Id, (ii, i) => new { ii, i });
            return await query
                .Where(x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0)
                .GroupBy(x => x.ii.PurchaseRequestItem != null ? x.ii.PurchaseRequestItem.ProductVariantId : (int?)null)
                .Select(
                    g => new ConfirmedInputSummary(
                        g.Key,
                        g.Sum(x => (long)(x.ii.Count ?? 0)),
                        g.Sum(
                            x => (x.ii.PurchaseRequestItem != null ? x.ii.PurchaseRequestItem.UnitPrice ?? 0m : 0m) *
                                (x.ii.Count ?? 0)),
                        g.Min(x => x.i.CreatedAt)))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        if (await TableExistsAsync("InventoryReceiptInfo", cancellationToken).ConfigureAwait(false) &&
            await TableExistsAsync("InventoryReceipt", cancellationToken).ConfigureAwait(false))
        {
            return await ReadConfirmedInputSummariesAsync(
                """
                    SELECT
                        [Info].[ProductVariantId] AS [VariantId],
                        SUM(CAST(ISNULL([Info].[Count], 0) AS bigint)) AS [TotalIn],
                        CAST(0 AS decimal(18, 2)) AS [WeightedInputTotal],
                        MIN([Receipt].[CreatedAt]) AS [FirstCreatedAt]
                    FROM [dbo].[InventoryReceiptInfo] AS [Info]
                    INNER JOIN [dbo].[InventoryReceipt] AS [Receipt]
                        ON [Info].[InventoryReceiptId] = [Receipt].[Id]
                    WHERE [Info].[ProductVariantId] IS NOT NULL
                      AND [Receipt].[StatusId] = @FinishedStatus
                      AND (@IncludeDeleted = 1 OR ([Info].[DeletedAt] IS NULL AND [Receipt].[DeletedAt] IS NULL))
                    GROUP BY [Info].[ProductVariantId]
                    """,
                includeDeleted,
                cancellationToken)
                .ConfigureAwait(false);
        }
        if (await TableExistsAsync("InputInfo", cancellationToken).ConfigureAwait(false) &&
            await TableExistsAsync("Input", cancellationToken).ConfigureAwait(false))
        {
            return await ReadConfirmedInputSummariesAsync(
                """
                    SELECT
                        [Info].[ProductId] AS [VariantId],
                        SUM(CAST(ISNULL([Info].[Count], 0) AS bigint)) AS [TotalIn],
                        SUM(CAST(ISNULL([Info].[InputPrice], 0) AS decimal(18, 2)) * CAST(ISNULL([Info].[Count], 0) AS decimal(18, 2))) AS [WeightedInputTotal],
                        MIN([Receipt].[CreatedAt]) AS [FirstCreatedAt]
                    FROM [dbo].[InputInfo] AS [Info]
                    INNER JOIN [dbo].[Input] AS [Receipt]
                        ON [Info].[InputId] = [Receipt].[Id]
                    WHERE [Info].[ProductId] IS NOT NULL
                      AND [Receipt].[StatusId] = @FinishedStatus
                      AND (@IncludeDeleted = 1 OR ([Info].[DeletedAt] IS NULL AND [Receipt].[DeletedAt] IS NULL))
                    GROUP BY [Info].[ProductId]
                    """,
                includeDeleted,
                cancellationToken)
                .ConfigureAwait(false);
        }
        return [];
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State is not ConnectionState.Open;
        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT CASE WHEN OBJECT_ID(@TableName, N'U') IS NULL THEN 0 ELSE 1 END";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@TableName";
            parameter.Value = $"[dbo].[{tableName}]";
            command.Parameters.Add(parameter);
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result is int value && value == 1;
        } finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task<List<ConfirmedInputSummary>> ReadConfirmedInputSummariesAsync(
        string commandText,
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var results = new List<ConfirmedInputSummary>();
        var connection = context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State is not ConnectionState.Open;
        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.Parameters.Add(CreateParameter(command, "@FinishedStatus", InventoryReceiptStatus.Approve));
            command.Parameters.Add(CreateParameter(command, "@IncludeDeleted", includeDeleted ? 1 : 0));
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(
                    new ConfirmedInputSummary(
                        reader.IsDBNull(0) ? null : reader.GetInt32(0),
                        reader.IsDBNull(1) ? 0 : reader.GetInt64(1),
                        reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                        reader.IsDBNull(3) ? null : reader.GetFieldValue<DateTimeOffset>(3)));
            }
        } finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
        return results;
    }

    private static DbParameter CreateParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        return parameter;
    }

    private sealed record ConfirmedInputSummary(
        int? VariantId,
        long TotalIn,
        decimal WeightedInputTotal,
        DateTimeOffset? FirstCreatedAt)
    {
        public decimal AverageInputPrice => TotalIn == 0 ? 0 : WeightedInputTotal / TotalIn;
    }

    public async Task<CustomerAnalyticsResponse> GetCustomerAnalyticsAsync(CancellationToken cancellationToken)
    {
        var leads = await context.Leads.IgnoreQueryFilters().ToListAsync(cancellationToken);
        var orders = await context.OutputOrders.IgnoreQueryFilters().ToListAsync(cancellationToken);
        var totalLeads = leads.Count;
        var hotLeads = leads.Count(l => l.Score >= 80);
        var newCustomers = orders.Select(o => o.CustomerPhone).Distinct().Count();
        var leadList = leads
            .OrderByDescending(l => l.CreatedAt)
            .Select(
                l => new CustomerLeadDto
                {
                    Id = l.Id,
                    CustomerName = l.FullName,
                    PhoneNumber = l.PhoneNumber,
                    Source =
                        l.Source switch
                            {
                                "WebStore" => "Website",
                                "Facebook" => "Facebook",
                                "Shop" => "Showroom",
                                _ => l.Source
                            },
                    LeadScore = l.Score,
                    Status =
                        l.Status switch
                            {
                                "New" => "Mới",
                                "Consulting" => "Đang theo dõi",
                                "Converted" => "Đã chuyển đổi",
                                "Won" => "Đã chuyển đổi",
                                "Lost" => "Không quan tâm",
                                "TestDriving" => "Đang theo dõi",
                                "Negotiating" => "Đang đàm phán",
                                _ => l.Status
                            },
                    LastContact = l.UpdatedAt ?? l.CreatedAt
                })
            .ToList();
        return new CustomerAnalyticsResponse
        {
            Kpi = new CustomerKpi { TotalLeads = totalLeads, NewCustomers = newCustomers, HotLeads = hotLeads },
            Leads = leadList
        };
    }

    public async Task<CustomerServiceAnalyticsResponse> GetCustomerServiceAnalyticsAsync(
        CancellationToken cancellationToken)
    {
        var contacts = await context.Contacts
            .IgnoreQueryFilters()
            .Include(c => c.Replies)
            .ToListAsync(cancellationToken);
        var totalWithRating = contacts.Where(c => c.Rating != null).ToList();
        double avgRating = totalWithRating.Count > 0 ? totalWithRating.Average(c => c.Rating.GetValueOrDefault()) : 5.0;
        var newComplaints = contacts.Count(c => c.Status == "Pending");
        var resolvedCount = contacts.Count(c => c.Status == "Closed");
        double avgResponseHours = 0;
        var repliedContacts = contacts.Where(
            c => c.Replies != null && c.Replies.Any(r => r.CreatedAt != null) && c.CreatedAt != null)
            .ToList();
        if (repliedContacts.Count > 0)
        {
            avgResponseHours = repliedContacts.Average(
                c =>
                {
                    var firstReply = c.Replies.Where(r => r.CreatedAt != null).OrderBy(r => r.CreatedAt).First();
                    return (firstReply.CreatedAt!.Value - c.CreatedAt!.Value).TotalHours;
                });
        }
        var complaintList = contacts
            .OrderByDescending(c => c.CreatedAt)
            .Select(
                c =>
                {
                    double? respHours = null;
                    if (c.Replies != null && c.Replies.Any(r => r.CreatedAt != null) && c.CreatedAt != null)
                    {
                        var firstReply = c.Replies.Where(r => r.CreatedAt != null).OrderBy(r => r.CreatedAt).First();
                        respHours = Math.Round((firstReply.CreatedAt!.Value - c.CreatedAt!.Value).TotalHours, 1);
                    }
                    string statusVN = c.Status switch
                    {
                        "Pending" => "Mới",
                        "Replied" => "Đã phản hồi",
                        "Closed" => "Đã đóng",
                        _ => c.Status
                    };
                    return new CustomerComplaintDto
                    {
                        Id = c.Id,
                        TicketCode = $"LH{c.Id}",
                        CustomerName = c.FullName,
                        Subject = c.Subject,
                        Rating = c.Rating ?? 0,
                        Status = statusVN,
                        CreatedAt = c.CreatedAt,
                        ResponseHours = respHours
                    };
                })
            .ToList();
        return new CustomerServiceAnalyticsResponse
        {
            Kpi =
                new CustomerServiceKpi
                {
                    AvgRating = Math.Round(avgRating, 1),
                    NewComplaints = newComplaints,
                    AvgResponseHours = Math.Round(avgResponseHours, 1),
                    ResolvedCount = resolvedCount
                },
            Complaints = complaintList
        };
    }

    public async Task<IEnumerable<RevenueByCategoryResponse>> GetRevenueByCategoryAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var endAdjusted = end.Hour == 0 && end.Minute == 0 && end.Second == 0
            ? end.Date.AddDays(1).AddTicks(-1)
            : end;
        var data = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= start &&
                    x.o.CreatedAt <= endAdjusted &&
                    x.o.DeletedAt == null &&
                    x.oi.DeletedAt == null &&
                    x.o.StatusId != null &&
                    x.o.StatusId.ToLower() == OrderStatus.Completed)
            .Select(
                x => new
                {
                    CategoryName = x.oi.ProductVariant != null &&
                                x.oi.ProductVariant.Product != null &&
                                x.oi.ProductVariant.Product.ProductCategory != null
                        ? x.oi.ProductVariant.Product.ProductCategory.Name
                        : "Khác",
                    Revenue = (x.oi.Price ?? 0M) * (x.oi.Count ?? 0)
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        decimal totalRevenue = data.Sum(d => d.Revenue);
        return data
      .GroupBy(d => d.CategoryName)
            .Select(
                g => new RevenueByCategoryResponse
                {
                    CategoryName = g.Key ?? "Unknown",
                    Revenue = g.Sum(x => x.Revenue),
                    Percentage = totalRevenue > 0 ? Math.Round(g.Sum(x => x.Revenue) / totalRevenue * 100, 1) : 0
                })
            .OrderByDescending(r => r.Revenue)
            .ToList();
    }

    public async Task<IEnumerable<DailyCategoryRevenueResponse>> GetDailyCategoryRevenueAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var endAdjusted = end.Hour == 0 && end.Minute == 0 && end.Second == 0
            ? end.Date.AddDays(1).AddTicks(-1)
            : end;
        var days = (endAdjusted - start).Days;
        if (days <= 0)
            days = 1;
        var startDate = DateOnly.FromDateTime(start.DateTime);
        var raw = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt != null &&
                    x.o.CreatedAt >= start &&
                    x.o.CreatedAt <= endAdjusted &&
                    x.o.DeletedAt == null &&
                    x.oi.DeletedAt == null &&
                    x.o.StatusId != null &&
                    x.o.StatusId.ToLower() == OrderStatus.Completed)
            .Select(
                x => new
                {
                    Day = DateOnly.FromDateTime(x.o.CreatedAt!.Value.DateTime),
                    CategoryName = x.oi.ProductVariant != null &&
                                x.oi.ProductVariant.Product != null &&
                                x.oi.ProductVariant.Product.ProductCategory != null
                        ? x.oi.ProductVariant.Product.ProductCategory.Name
                        : "Khác",
                    Revenue = (x.oi.Price ?? 0M) * (x.oi.Count ?? 0)
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var dates = Enumerable.Range(0, days + 1).Select(i => startDate.AddDays(i)).ToList();
        return dates.SelectMany(
            d => raw
      .Where(r => r.Day == d)
                .GroupBy(r => r.CategoryName)
                .Select(
                    g => new DailyCategoryRevenueResponse
                        {
                            ReportDay = d.ToString("dd/MM"),
                            CategoryName = g.Key ?? "Unknown",
                            Revenue = g.Sum(x => x.Revenue)
                        }))
            .ToList();
    }

    public async Task<IEnumerable<StaffPerformanceResponse>> GetTopStaffPerformanceAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        int limit,
        CancellationToken cancellationToken)
    {
        var endAdjusted = end.Hour == 0 && end.Minute == 0 && end.Second == 0
            ? end.Date.AddDays(1).AddTicks(-1)
            : end;

        var rawData = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= start &&
                    x.o.CreatedAt <= endAdjusted &&
                    (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .Select(
                x => new
                {
                    StaffName = x.o.FinishedByUser != null
                        ? x.o.FinishedByUser.FullName
                        : (x.o.CreatedByUser != null ? x.o.CreatedByUser.FullName : "Bán hàng Online"),
                    Revenue = (x.oi.Price ?? 0) * (x.oi.Count ?? 0)
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var data = rawData
            .GroupBy(x => x.StaffName)
            .Select(
                g => new StaffPerformanceResponse
                {
                    EmployeeName = g.Key,
                    Role = g.Key == "Bán hàng Online" ? "Kênh trực tuyến" : "Nhân viên bán hàng",
                    TotalSales = g.Sum(x => x.Revenue),
                    TargetSales = 50000000M,
                    CommissionPaid = g.Sum(x => x.Revenue) * 0.05M,
                    KpiStatus = g.Sum(x => x.Revenue) >= 50000000M ? "Vượt KPI" : (g.Sum(x => x.Revenue) >= 25000000M ? "Đạt" : "Cần cải thiện"),
                    IsTopSeller = false
                })
            .OrderByDescending(x => x.TotalSales)
            .Take(limit)
            .ToList();

        if (data.Count > 0)
            data[0].IsTopSeller = true;

        return data;
    }

    public async Task<IEnumerable<TransactionLogResponse>> GetRecentTransactionsAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        var orders = await context.OutputOrders
            .IgnoreQueryFilters()
            .Select(
                o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.CustomerName,
                    BuyerName = o.Buyer != null ? o.Buyer.FullName : null,
                    CreatedByName = o.CreatedByUser != null ? o.CreatedByUser.FullName : null,
                    FinishedByName = o.FinishedByUser != null ? o.FinishedByUser.FullName : null,
                    o.ShippingFee,
                    o.StatusId
                })
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var orderIds = orders.Select(o => o.Id).ToList();
        if (orderIds.Count == 0)
            return [];

        var itemSummaries = await context.OutputInfos
            .IgnoreQueryFilters()
            .Where(oi => orderIds.Contains(oi.OutputId))
            .Select(
                oi => new
                {
                    oi.OutputId,
                    ProductName = oi.ProductVariant != null && oi.ProductVariant.Product != null
                        ? oi.ProductVariant.Product.Name
                        : null,
                    oi.DeletedAt,
                    Amount = (oi.Price ?? 0) * (oi.Count ?? 0)
                })
            .GroupBy(oi => oi.OutputId)
            .Select(
                g => new
                {
                    OutputId = g.Key,
                    FirstProductName = g.Select(x => x.ProductName).FirstOrDefault(x => x != null),
                    ItemCount = g.Count(),
                    Amount = g.Where(x => x.DeletedAt == null).Sum(x => x.Amount)
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var itemSummaryByOrderId = itemSummaries.ToDictionary(x => x.OutputId);
        return orders
            .Select(
                o =>
                {
                    itemSummaryByOrderId.TryGetValue(o.Id, out var itemSummary);
                    var firstItem = itemSummary?.FirstProductName;
                    var extraCount = (itemSummary?.ItemCount ?? 0) - 1;
                    var prodName = firstItem != null
                        ? (extraCount > 0 ? $"{firstItem} (+{extraCount} món)" : firstItem)
                        : "Sản phẩm";

                    var custName = !string.IsNullOrWhiteSpace(o.CustomerName)
                        ? o.CustomerName
                        : (o.BuyerName ?? "Khách lẻ");

                    var staff = o.FinishedByName ?? o.CreatedByName ?? "Hệ thống";

                    return new TransactionLogResponse
                    {
                        Timestamp = o.CreatedAt?.DateTime ?? DateTime.UtcNow,
                        CustomerName = custName,
                        ProductName = prodName,
                        Amount = (itemSummary?.Amount ?? 0) + (o.ShippingFee ?? 0),
                        IsRevenue = true,
                        Status = o.StatusId ?? string.Empty,
                        StaffName = staff
                    };
                })
            .ToList();
    }

    private static (DateTimeOffset Start, DateTimeOffset End) KpiRange(string period)
    {
        var vietnamOffset = TimeSpan.FromHours(7);
        var now = DateTimeOffset.UtcNow.ToOffset(vietnamOffset);
        var p = period.ToLower();
        if (p == "today")
            return (new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, vietnamOffset), now);
        if (p == "week")
        {
            var d = (int)now.DayOfWeek;
            var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, vietnamOffset);
            var mon = today.AddDays(d == 0 ? -6 : 1 - d);
            return (mon, now);
        }
        if (p == "year")
            return (new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, vietnamOffset),
                now);
        return (new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, vietnamOffset), now);
    }

    private static (DateTimeOffset Start, DateTimeOffset End) KpiPrev(string period, DateTimeOffset s, DateTimeOffset e)
    {
        var p = period.ToLower();
        if (p == "today")
            return (s.AddDays(-1), e.AddDays(-1));
        if (p == "week")
            return (s.AddDays(-7), e.AddDays(-7));
        if (p == "year")
            return (s.AddYears(-1), e.AddYears(-1));
        var previousStart = s.AddMonths(-1);
        var previousEnd = previousStart.Add(e - s);
        var previousMonthEnd = s.AddTicks(-1);
        return (previousStart, previousEnd < previousMonthEnd ? previousEnd : previousMonthEnd);
    }

    private static double Pct(int c, int pv) => pv > 0 ? Math.Round((double)(c - pv) / pv * 100, 1) : 0;

    public async Task<DashboardKpisResponse> GetDashboardKpisAsync(string period, CancellationToken cancellationToken)
    {
        var (s, e) = KpiRange(period);
        var (ps, pe) = KpiPrev(period, s, e);
        var lbl = period.ToLower() switch
        {
            "today" => "Hom nay",
            "week" => "Tuan nay",
            "year" => "Nam nay",
            _ => "Thang nay"
        };
        var orders = await context.OutputOrders
            .IgnoreQueryFilters()
            .Where(o => o.CreatedAt >= s && o.CreatedAt <= e)
            .CountAsync(cancellationToken);
        var prevOrd = await context.OutputOrders
            .IgnoreQueryFilters()
            .Where(o => o.CreatedAt >= ps && o.CreatedAt <= pe)
            .CountAsync(cancellationToken);
        var custs = await context.CustomerContacts
            .IgnoreQueryFilters()
            .Where(c => c.CreatedAt >= s && c.CreatedAt <= e)
            .CountAsync(cancellationToken);
        var prevCst = await context.CustomerContacts
            .IgnoreQueryFilters()
            .Where(c => c.CreatedAt >= ps && c.CreatedAt <= pe)
            .CountAsync(cancellationToken);
        var appts = await context.BookingAppointments
            .IgnoreQueryFilters()
            .Where(a => a.AppointmentAt >= s && a.AppointmentAt <= e)
            .CountAsync(cancellationToken);
        var pending = await context.OutputOrders
            .IgnoreQueryFilters()
            .Where(
                o => o.StatusId == "pending" || o.StatusId == "waiting_deposit" || o.StatusId == "waiting_installment")
            .CountAsync(cancellationToken);
        var overdue = await context.FinanceContracts
            .IgnoreQueryFilters()
            .CountAsync(f => f.DisbursementStatus == "default" || f.DisbursementStatus == "overdue", cancellationToken);
        var lowV = await context.Vehicles.IgnoreQueryFilters().Where(v => v.IsActive).CountAsync(cancellationToken);
        var lowP = await context.InventoryOnHands
            .IgnoreQueryFilters()
            .Where(i => i.StockQty < 10)
            .CountAsync(cancellationToken);
        var comp = await context.Set<Domain.Entities.CustomerFeedback>()
            .IgnoreQueryFilters()
            .Where(f => f.CreatedAt >= s && f.CreatedAt <= e)
            .CountAsync(cancellationToken);
        var missed = await context.Set<Domain.Entities.BookingAppointment>()
            .IgnoreQueryFilters()
            .Where(a => a.Status == "no_show" && a.AppointmentAt >= s && a.AppointmentAt <= e)
            .CountAsync(cancellationToken);
        var cards = new List<CardItem>
        {
            new("Đơn hàng mới", orders, Pct(orders, prevOrd), "ri:file-list-3-line", "don"),
            new("Khách hàng mới", custs, Pct(custs, prevCst), "ri:user-add-line", "nguoi"),
            new("Lịch hẹn trong kỳ", appts, 0, "ri:calendar-check-line", "lich"),
            new("Đơn hàng quá hạn", pending, 0, "ri:alert-line", "don"),
        };
        var alerts = new AlertsSummary(
            new FinancialAlerts(overdue, false),
            new InventoryAlerts(lowV, lowP),
            new CustomerAlerts(comp, missed),
            new OperationsAlerts(pending));
        return new DashboardKpisResponse(lbl, s.ToString("dd/MM/yyyy"), e.ToString("dd/MM/yyyy"), cards, alerts);
    }
}
