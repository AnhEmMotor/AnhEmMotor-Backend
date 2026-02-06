using Application.ApiContracts.Statistical.Responses;
using Domain.Constants.Order;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit.Abstractions;
using BrandEntity = Domain.Entities.Brand;
using InputEntity = Domain.Entities.Input;
using InputInfoEntity = Domain.Entities.InputInfo;
using InputStatusEntity = Domain.Entities.InputStatus;
using OutputEntity = Domain.Entities.Output;
using OutputInfoEntity = Domain.Entities.OutputInfo;
using OutputStatusEntity = Domain.Entities.OutputStatus;
using ProductCategoryEntity = Domain.Entities.ProductCategory;
using ProductEntity = Domain.Entities.Product;
using ProductStatusEntity = Domain.Entities.ProductStatus;

namespace IntegrationTests;

[Collection("Shared Integration Collection")]
public class Statistics : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public Statistics(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    { await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(false); }

    private static async Task WipeStatisticsDataAsync(
        ApplicationDBContext db,
        CancellationToken cancellationToken = default)
    {
        db.OutputInfos.RemoveRange(db.OutputInfos);
        db.OutputOrders.RemoveRange(db.OutputOrders);
        db.InputInfos.RemoveRange(db.InputInfos);
        db.InputReceipts.RemoveRange(db.InputReceipts);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"manager_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "StrongPass1@",
            [ PermissionsList.Statistical.View ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var token = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "StrongPass1@",
            cancellationToken)
            .ConfigureAwait(false)).AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return uniqueId;
    }

    private static async Task SeedPrerequisitesAsync(
        ApplicationDBContext db,
        CancellationToken cancellationToken = default)
    {
        if(!await db.OutputStatuses
            .AnyAsync(x => string.Compare(x.Key, OrderStatus.Pending) == 0, cancellationToken)
            .ConfigureAwait(false))
            db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Pending });
        if(!await db.OutputStatuses
            .AnyAsync(x => string.Compare(x.Key, OrderStatus.Completed) == 0, cancellationToken)
            .ConfigureAwait(false))
            db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Completed });
        if(!await db.OutputStatuses
            .AnyAsync(x => string.Compare(x.Key, OrderStatus.Cancelled) == 0, cancellationToken)
            .ConfigureAwait(false))
            db.OutputStatuses.Add(new OutputStatusEntity { Key = OrderStatus.Cancelled });
        if(!await db.ProductStatuses
            .AnyAsync(x => string.Compare(x.Key, "ForSale") == 0, cancellationToken)
            .ConfigureAwait(false))
            db.ProductStatuses.Add(new ProductStatusEntity { Key = "ForSale" });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
    }

    private static async Task<int> SeedProductVariantAsync(
        ApplicationDBContext db,
        string uniqueId,
        decimal price = 200000,
        CancellationToken cancellationToken = default)
    {
        var brand = new BrandEntity { Name = $"Brand_{uniqueId}" };
        db.Brands.Add(brand);
        var category = new ProductCategoryEntity { Name = $"Cat_{uniqueId}" };
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var product = new ProductEntity
        {
            Name = $"Prod_{uniqueId}",
            BrandId = brand.Id,
            CategoryId = category.Id,
            StatusId = "ForSale"
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var variant = new ProductVariant { ProductId = product.Id, Price = price, UrlSlug = $"slug-{uniqueId}" };
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return variant.Id;
    }

#pragma warning disable IDE0079
#pragma warning disable CRR0035
    [Fact(DisplayName = "STAT_021 - Lấy doanh thu 7 ngày gần nhất (Completed Only)")]
    public async Task GetDailyRevenue_Last7Days_ReturnsCorrectData()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var variantId = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var today = DateTime.UtcNow;
        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = today, Notes = "Order1" };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = variantId, Price = 600000, Count = 2 });

        var o2 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = today.AddDays(-1), Notes = "Order2" };
        db.OutputOrders.Add(o2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o2.Id, ProductVarientId = variantId, Price = 3500000, Count = 1 });

        var o3 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = today.AddDays(-2), Notes = "Order3" };
        db.OutputOrders.Add(o3);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o3.Id, ProductVarientId = variantId, Price = 2800000, Count = 1 });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=5").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<List<DailyRevenueResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.Count.Should().Be(5);
        content.First(x => x.ReportDay == DateOnly.FromDateTime(today)).TotalRevenue.Should().Be(1200000);
        content.First(x => x.ReportDay == DateOnly.FromDateTime(today.AddDays(-1))).TotalRevenue.Should().Be(3500000);
        content.First(x => x.ReportDay == DateOnly.FromDateTime(today.AddDays(-2))).TotalRevenue.Should().Be(2800000);
        content.First(x => x.ReportDay == DateOnly.FromDateTime(today.AddDays(-3))).TotalRevenue.Should().Be(0);
    }

    [Fact(DisplayName = "STAT_022 - Lấy doanh thu chỉ tính đơn hàng hợp lệ (Skip Cancelled/Pending)")]
    public async Task GetDailyRevenue_FiltersStatus_Correctly()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var variantId = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var today = DateTime.UtcNow;
        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = today };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = variantId, Price = 2000000, Count = 1 });

        var o2 = new OutputEntity { StatusId = OrderStatus.Cancelled, CreatedAt = today };
        db.OutputOrders.Add(o2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o2.Id, ProductVarientId = variantId, Price = 5000000, Count = 1 });

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=1").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<DailyRevenueResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.First().TotalRevenue.Should().Be(2000000);
    }

    [Fact(DisplayName = "STAT_023 - Lấy doanh thu với múi giờ UTC")]
    public async Task GetDailyRevenue_Timezone_UTC()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var variantId = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var date = new DateTime(2025, 1, 1, 23, 30, 0, DateTimeKind.Utc);
        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = date };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = variantId, Price = 1000000, Count = 1 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var yest = DateTime.UtcNow.Date.AddDays(-1).AddHours(23).AddMinutes(30);
        var o2 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = yest };
        db.OutputOrders.Add(o2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o2.Id, ProductVarientId = variantId, Price = 500000, Count = 1 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=5").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<DailyRevenueResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        var targetDay = DateOnly.FromDateTime(yest);
        content!.FirstOrDefault(x => x.ReportDay == targetDay)?.TotalRevenue.Should().Be(500000);
    }

    [Fact(DisplayName = "STAT_024 - Lấy doanh thu dùng OutputInfo.Price")]
    public async Task GetDailyRevenue_UsesSnapshotPrice()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var variantId = await SeedProductVariantAsync(db, uniqueId, 1000000, CancellationToken.None)
            .ConfigureAwait(true);

        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = DateTime.UtcNow };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = variantId, Price = 800000, Count = 1 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=1").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<DailyRevenueResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.First().TotalRevenue.Should().Be(800000);
    }

    [Fact(DisplayName = "STAT_025 - Dashboard Stats Last Month")]
    public async Task GetDashboardStats_LastMonth_Calculations()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var variantId = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var today = DateTime.UtcNow;
        var lastMonthDate = new DateTime(today.Year, today.Month, 15, 12, 0, 0, DateTimeKind.Utc).AddMonths(-1);

        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = lastMonthDate };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(
                new OutputInfoEntity
                {
                    OutputId = o1.Id,
                    ProductVarientId = variantId,
                    Price = 30000000,
                    CostPrice = 18000000,
                    Count = 1
                });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/dashboard-stats").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<DashboardStatsResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.LastMonthRevenue.Should().Be(30000000);
        content.LastMonthProfit.Should().Be(12000000);
    }

    [Fact(DisplayName = "STAT_026 - Dashboard Pending Orders Count")]
    public async Task GetDashboardStats_PendingCount()
    {
        await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);

        for(int i = 0; i < 7; i++)
        {
            db.OutputOrders.Add(new OutputEntity { StatusId = OrderStatus.Pending, CreatedAt = DateTime.UtcNow });
        }
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/dashboard-stats").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<DashboardStatsResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.PendingOrdersCount.Should().Be(7);
    }

    [Fact(DisplayName = "STAT_027 - User mới đăng ký (Expect 0 due to hardcoded)")]
    public async Task GetDashboardStats_NewUsers()
    {
        await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/Statistics/dashboard-stats").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<DashboardStatsResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.NewCustomersCount.Should().Be(0);
    }

    [Fact(DisplayName = "STAT_028 - Monthly Revenue 6 Months")]
    public async Task GetMonthlyRevenue_Last6Months()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var variantId = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var currentMonthStart = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var d1 = DateTime.UtcNow.AddMonths(-1);
        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = d1 };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = variantId, Price = 1000000, Count = 1 });

        var d2 = DateTime.UtcNow.AddMonths(-3);
        var o2 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = d2 };
        db.OutputOrders.Add(o2);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o2.Id, ProductVarientId = variantId, Price = 2000000, Count = 1 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/monthly-revenue-profit?months=6").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<MonthlyRevenueProfitResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.Count.Should().Be(6);
        content.First(x => x.ReportMonth == currentMonthStart.AddMonths(-1)).TotalRevenue.Should().Be(1000000);
        content.First(x => x.ReportMonth == currentMonthStart.AddMonths(-3)).TotalRevenue.Should().Be(2000000);
    }

    [Fact(DisplayName = "STAT_029 - Lợi nhuận khi CostPrice = 0")]
    public async Task GetMonthlyRevenue_ZeroCost()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var variantId = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = DateTime.UtcNow };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(
                new OutputInfoEntity
                {
                    OutputId = o1.Id,
                    ProductVarientId = variantId,
                    Price = 1000000,
                    CostPrice = 0,
                    Count = 1
                });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/monthly-revenue-profit?months=1").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<MonthlyRevenueProfitResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.First().TotalProfit.Should().Be(1000000);
    }

    [Fact(DisplayName = "STAT_030 - Order Status Counts (All Statuses)")]
    public async Task GetOrderStatusCounts_AllStatuses()
    {
        await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);

        for(int i = 0; i < 3; i++)
            db.OutputOrders.Add(new OutputEntity { StatusId = OrderStatus.Pending, CreatedAt = DateTime.UtcNow });
        for(int i = 0; i < 10; i++)
            db.OutputOrders.Add(new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = DateTime.UtcNow });
        db.OutputOrders.Add(new OutputEntity { StatusId = OrderStatus.Cancelled, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/order-status-counts").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<OrderStatusCountResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content?.First(x => string.Compare(x.StatusName, OrderStatus.Pending, StringComparison.Ordinal) == 0).OrderCount
        .Should()
        .Be(3);
        content?.First(x => string.Compare(x.StatusName, OrderStatus.Completed, StringComparison.Ordinal) == 0)
        .OrderCount
        .Should()
        .Be(10);
        content?.First(x => string.Compare(x.StatusName, OrderStatus.Cancelled, StringComparison.Ordinal) == 0)
        .OrderCount
        .Should()
        .Be(1);
    }

    [Fact(DisplayName = "STAT_031 - Báo cáo sản phẩm (Multi Variants)")]
    public async Task GetProductReport_Variants()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var v1 = await SeedProductVariantAsync(db, $"{uniqueId}1", cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);
        var v2 = await SeedProductVariantAsync(db, $"{uniqueId}2", cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var lastMonth = DateTime.UtcNow.AddMonths(-1);
        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = lastMonth };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos.Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = v1, Count = 20 });
        db.OutputInfos.Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = v2, Count = 15 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/product-report-last-month").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<ProductReportResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content?.First(x => x.VariantId == v1).SoldLastMonth.Should().Be(20);
        content?.First(x => x.VariantId == v2).SoldLastMonth.Should().Be(15);
    }

    [Fact(DisplayName = "STAT_032 - Báo cáo gồm sản phẩm đã xóa (Soft Delete)")]
    public async Task GetProductReport_IncludeDeleted()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var vid = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var v = await db.ProductVariants.FindAsync(vid).ConfigureAwait(true);
        v!.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = DateTime.UtcNow.AddMonths(-1) };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos.Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = vid, Count = 5 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/product-report-last-month").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<ProductReportResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.Any(x => x.VariantId == vid).Should().BeTrue();
    }

    [Fact(DisplayName = "STAT_033 - Tồn kho và giá thập phân")]
    public async Task GetStockPrice_Decimal()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);

        decimal price = 1250750.50m;
        var vid = await SeedProductVariantAsync(db, uniqueId, price, CancellationToken.None).ConfigureAwait(true);

        if(!await db.InputStatuses
            .AnyAsync(
                x => string.Compare(x.Key, Domain.Constants.Input.InputStatus.Finish) == 0,
                CancellationToken.None)
            .ConfigureAwait(true))
        {
            db.InputStatuses.Add(new InputStatusEntity { Key = Domain.Constants.Input.InputStatus.Finish });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var inp = new InputEntity { StatusId = Domain.Constants.Input.InputStatus.Finish, CreatedAt = DateTime.UtcNow };
        db.InputReceipts.Add(inp);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.InputInfos.Add(new InputInfoEntity { InputId = inp.Id, ProductId = vid, Count = 35 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/Statistics/product-stock-price/{vid}").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<ProductStockPriceResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.UnitPrice.Should().Be(price);
        content.StockQuantity.Should().Be(35);
    }

    [Fact(DisplayName = "STAT_034 - Tồn kho variant đã xóa (Expect 404 per current code)")]
    public async Task GetStockPrice_Deleted_Returns404()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var vid = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var v = await db.ProductVariants.FindAsync(vid).ConfigureAwait(true);
        v!.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/Statistics/product-stock-price/{vid}").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "STAT_035 - Doanh thu nhiều OutputInfo")]
    public async Task GetDailyRevenue_MultipleItems()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var vid = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = DateTime.UtcNow };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos.Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = vid, Count = 2, Price = 500000 });
        db.OutputInfos
            .Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = vid, Count = 1, Price = 1200000 });
        db.OutputInfos.Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = vid, Count = 3, Price = 300000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=1").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<DailyRevenueResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.First().TotalRevenue.Should().Be(3100000);
    }

    [Fact(DisplayName = "STAT_037 - Lợi nhuận âm")]
    public async Task GetMonthlyRevenue_NegativeProfit()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var vid = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = DateTime.UtcNow };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos
            .Add(
                new OutputInfoEntity
                {
                    OutputId = o1.Id,
                    ProductVarientId = vid,
                    Count = 1,
                    Price = 1000000,
                    CostPrice = 1500000
                });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/monthly-revenue-profit?months=1").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<MonthlyRevenueProfitResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.First().TotalProfit.Should().Be(-500000);
    }

    [Fact(DisplayName = "STAT_038 - Tháng không có đơn hàng (Revenue = 0)")]
    public async Task GetMonthlyRevenue_GapMonths()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var vid = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var d2 = DateTime.UtcNow.AddMonths(-2);
        var o1 = new OutputEntity { StatusId = OrderStatus.Completed, CreatedAt = d2 };
        db.OutputOrders.Add(o1);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        db.OutputInfos.Add(new OutputInfoEntity { OutputId = o1.Id, ProductVarientId = vid, Count = 1, Price = 100000 });
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/monthly-revenue-profit?months=3").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<MonthlyRevenueProfitResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        var currentMonthStart = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthGap = currentMonthStart.AddMonths(-1);

        content!.First(x => x.ReportMonth == monthGap).TotalRevenue.Should().Be(0);
    }

    [Fact(DisplayName = "STAT_039 - Không có đơn nào")]
    public async Task GetOrderStatusCounts_EmptyDB()
    {
        await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/Statistics/order-status-counts").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<OrderStatusCountResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content.Should().NotBeNull();
        content!.All(x => x.OrderCount == 0).Should().BeTrue();
    }

    [Fact(DisplayName = "STAT_040 - Báo cáo sản phẩm khi không bán")]
    public async Task GetProductReport_NoSales()
    {
        var uniqueId = await AuthenticateAsync(CancellationToken.None).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        await WipeStatisticsDataAsync(db, CancellationToken.None).ConfigureAwait(true);
        await SeedPrerequisitesAsync(db, CancellationToken.None).ConfigureAwait(true);
        var vid = await SeedProductVariantAsync(db, uniqueId, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/Statistics/product-report-last-month").ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<ProductReportResponse>>(CancellationToken.None)
            .ConfigureAwait(true);

        content!.First(x => x.VariantId == vid).SoldLastMonth.Should().Be(0);
        content!.First(x => x.VariantId == vid).StockQuantity.Should().Be(0);
    }
}
