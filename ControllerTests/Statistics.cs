using Application.ApiContracts.Statistical.Responses;
using Application.Features.Statistical.Queries.GetAdminDashboardOverview;
using Application.Features.Statistical.Queries.GetDailyRevenue;
using Application.Features.Statistical.Queries.GetDashboardStats;
using Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;
using Application.Features.Statistical.Queries.GetOrderStatusCounts;
using Application.Features.Statistical.Queries.GetProductReportLastMonth;
using Application.Features.Statistical.Queries.GetProductStockAndPrice;
using Application.Interfaces.Repositories.Statistical;
using Domain.Constants.Order;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class Statistics
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IStatisticalReadRepository> _repositoryMock;
    private readonly StatisticsController _controller;

    public Statistics()
    {
        _mediatorMock = new Mock<IMediator>();
        _repositoryMock = new Mock<IStatisticalReadRepository>();
        _controller = new StatisticsController(_mediatorMock.Object, _repositoryMock.Object, null!);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(DisplayName = "STAT_001 - Lấy doanh thu theo ngày - Happy Path với 7 ngày")]
    public async Task GetDailyRevenue_ValidDays7_ReturnsRevenueData()
    {
        var days = 7;
        var expectedRevenue = new List<DailyRevenueResponse>
        {
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-6)), TotalRevenue = 3000000 },
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)), TotalRevenue = 2000000 },
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-4)), TotalRevenue = 3600000 },
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)), TotalRevenue = 0 },
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)), TotalRevenue = 3400000 },
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), TotalRevenue = 0 },
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now), TotalRevenue = 3500000 }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDailyRevenueQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRevenue);
        var result = await _controller.GetDailyRevenueAsync(days, CancellationToken.None).ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualRevenue = okResult.Value.Should().BeAssignableTo<IEnumerable<DailyRevenueResponse>>().Subject;
        actualRevenue.Should().HaveCount(7);
        actualRevenue.Sum(x => x.TotalRevenue).Should().Be(15500000);
    }

    [Fact(DisplayName = "STAT_002 - Lấy doanh thu theo ngày - Không có quyền")]
    public async Task GetDailyRevenue_NoPermission_ReturnsForbidden()
    {
        var days = 7;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDailyRevenueQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền truy cập"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetDailyRevenueAsync(days, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_003 - Lấy doanh thu theo ngày - Chưa đăng nhập")]
    public async Task GetDailyRevenue_NotAuthenticated_ReturnsUnauthorized()
    {
        var days = 7;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDailyRevenueQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Yêu cầu đăng nhập"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetDailyRevenueAsync(days, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_004 - Lấy doanh thu theo ngày - Days âm")]
    public async Task GetDailyRevenue_NegativeDays_ReturnsBadRequest()
    {
        var days = -5;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDailyRevenueQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("days phải lớn hơn 0"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.GetDailyRevenueAsync(days, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_005 - Lấy doanh thu theo ngày - Days = 0")]
    public async Task GetDailyRevenue_ZeroDays_ReturnsBadRequest()
    {
        var days = 0;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDailyRevenueQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("days phải lớn hơn 0"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.GetDailyRevenueAsync(days, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_006 - Lấy doanh thu theo ngày - Days quá lớn (366)")]
    public async Task GetDailyRevenue_DaysTooLarge_ReturnsBadRequest()
    {
        var days = 366;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDailyRevenueQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("days không được vượt quá 365"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.GetDailyRevenueAsync(days, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_007 - Lấy chỉ số Dashboard - Happy Path")]
    public async Task GetDashboardStats_ValidRequest_ReturnsDashboardStats()
    {
        var expectedStats = new DashboardStatsResponse
        {
            LastMonthRevenue = 50000000,
            LastMonthProfit = 12000000,
            PendingOrdersCount = 5,
            NewCustomersCount = 10
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);
        var result = await _controller.GetDashboardStatsAsync(CancellationToken.None).ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualStats = okResult.Value.Should().BeAssignableTo<DashboardStatsResponse>().Subject;
        actualStats.LastMonthRevenue.Should().Be(50000000);
        actualStats.LastMonthProfit.Should().Be(12000000);
        actualStats.PendingOrdersCount.Should().Be(5);
        actualStats.NewCustomersCount.Should().Be(10);
    }

    [Fact(DisplayName = "STAT_008 - Lấy chỉ số Dashboard - Không có quyền")]
    public async Task GetDashboardStats_NoPermission_ReturnsForbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetDashboardStatsAsync(CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_009 - Lấy doanh thu và lợi nhuận theo tháng - Happy Path với 12 tháng")]
    public async Task GetMonthlyRevenueProfit_Valid12Months_ReturnsMonthlyData()
    {
        var months = 12;
        var expectedData = new List<MonthlyRevenueProfitResponse>();
        for (int i = 0; i < 12; i++)
        {
            expectedData.Add(
                new MonthlyRevenueProfitResponse
                {
                    ReportMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-i)),
                    TotalRevenue = (i + 1) * 1000000,
                    TotalProfit = (i + 1) * 300000
                });
        }
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMonthlyRevenueProfitQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);
        var result = await _controller.GetMonthlyRevenueProfitAsync(months, CancellationToken.None).ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualData = okResult.Value.Should().BeAssignableTo<IEnumerable<MonthlyRevenueProfitResponse>>().Subject;
        actualData.Should().HaveCount(12);
    }

    [Fact(DisplayName = "STAT_010 - Lấy doanh thu và lợi nhuận theo tháng - Months âm")]
    public async Task GetMonthlyRevenueProfit_NegativeMonths_ReturnsBadRequest()
    {
        var months = -3;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMonthlyRevenueProfitQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("months phải lớn hơn 0"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.GetMonthlyRevenueProfitAsync(months, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_011 - Lấy doanh thu và lợi nhuận theo tháng - Months = 0")]
    public async Task GetMonthlyRevenueProfit_ZeroMonths_ReturnsBadRequest()
    {
        var months = 0;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMonthlyRevenueProfitQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("months phải lớn hơn 0"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.GetMonthlyRevenueProfitAsync(months, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_012 - Lấy doanh thu và lợi nhuận theo tháng - Months quá lớn (25)")]
    public async Task GetMonthlyRevenueProfit_MonthsTooLarge_ReturnsBadRequest()
    {
        var months = 25;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMonthlyRevenueProfitQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("months không được vượt quá 24"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.GetMonthlyRevenueProfitAsync(months, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_013 - Lấy số lượng đơn hàng theo trạng thái - Happy Path")]
    public async Task GetOrderStatusCounts_ValidRequest_ReturnsStatusCounts()
    {
        var expectedCounts = new List<OrderStatusCountResponse>
        {
            new() { StatusName = "pending", OrderCount = 5 },
            new() { StatusName = "processing", OrderCount = 10 },
            new() { StatusName = "completed", OrderCount = 15 },
            new() { StatusName = "cancelled", OrderCount = 2 }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrderStatusCountsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCounts);
        var result = await _controller.GetOrderStatusCountsAsync(CancellationToken.None).ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualCounts = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderStatusCountResponse>>().Subject;
        actualCounts.Should().HaveCount(4);
        actualCounts.First(x => string.Compare(x.StatusName, "pending") == 0).OrderCount.Should().Be(5);
        actualCounts.First(x => string.Compare(x.StatusName, "completed") == 0).OrderCount.Should().Be(15);
    }

    [Fact(DisplayName = "STAT_014 - Lấy số lượng đơn hàng theo trạng thái - Không có quyền")]
    public async Task GetOrderStatusCounts_NoPermission_ReturnsForbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrderStatusCountsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetOrderStatusCountsAsync(CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_015 - Lấy báo cáo sản phẩm tháng trước - Happy Path")]
    public async Task GetProductReportLastMonth_ValidRequest_ReturnsProductReport()
    {
        var expectedReport = new List<ProductReportResponse>
        {
            new() { ProductName = "Sản phẩm A", VariantId = 1, SoldLastMonth = 50, StockQuantity = 100 },
            new() { ProductName = "Sản phẩm B", VariantId = 2, SoldLastMonth = 30, StockQuantity = 200 },
            new() { ProductName = "Sản phẩm C", VariantId = 3, SoldLastMonth = 0, StockQuantity = 150 }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductReportLastMonthQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReport);
        var result = await _controller.GetProductReportLastMonthAsync(CancellationToken.None).ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualReport = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductReportResponse>>().Subject;
        actualReport.Should().HaveCount(3);
        actualReport.First(x => string.Compare(x.ProductName, "Sản phẩm A") == 0).SoldLastMonth.Should().Be(50);
        actualReport.First(x => string.Compare(x.ProductName, "Sản phẩm B") == 0).StockQuantity.Should().Be(200);
    }

    [Fact(DisplayName = "STAT_016 - Lấy báo cáo sản phẩm tháng trước - Không có quyền")]
    public async Task GetProductReportLastMonth_NoPermission_ReturnsForbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductReportLastMonthQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetProductReportLastMonthAsync(CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_017 - Lấy giá và tồn kho sản phẩm - Happy Path")]
    public async Task GetProductStockAndPrice_ValidVariantId_ReturnsStockAndPrice()
    {
        var variantId = 10;
        var expectedResponse = new ProductStockPriceResponse { UnitPrice = 2500000, StockQuantity = 50 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductStockAndPriceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        var result = await _controller.GetProductStockAndPriceAsync(variantId, CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualResponse = okResult.Value.Should().BeAssignableTo<ProductStockPriceResponse>().Subject;
        actualResponse.UnitPrice.Should().Be(2500000);
        actualResponse.StockQuantity.Should().Be(50);
    }

    [Fact(DisplayName = "STAT_018 - Lấy giá và tồn kho sản phẩm - VariantId không tồn tại")]
    public async Task GetProductStockAndPrice_NonExistentVariantId_ReturnsNotFound()
    {
        var variantId = 999;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductStockAndPriceQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Không tìm thấy sản phẩm"));
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.GetProductStockAndPriceAsync(variantId, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_019 - Lấy giá và tồn kho sản phẩm - VariantId âm")]
    public async Task GetProductStockAndPrice_NegativeVariantId_ReturnsBadRequest()
    {
        var variantId = -5;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductStockAndPriceQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("variantId không hợp lệ"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.GetProductStockAndPriceAsync(variantId, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "STAT_020 - Lấy giá và tồn kho sản phẩm - Variant đã bị xóa mềm")]
    public async Task GetProductStockAndPrice_SoftDeletedVariant_ReturnsStockAndPrice()
    {
        var variantId = 15;
        var expectedResponse = new ProductStockPriceResponse { UnitPrice = 500000, StockQuantity = 0 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProductStockAndPriceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        var result = await _controller.GetProductStockAndPriceAsync(variantId, CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualResponse = okResult.Value.Should().BeAssignableTo<ProductStockPriceResponse>().Subject;
        actualResponse.UnitPrice.Should().Be(500000);
        actualResponse.StockQuantity.Should().Be(0);
    }

    [Fact(DisplayName = "STAT_104 - Controller - Kiểm tra danh sách sản phẩm bán chạy nhất")]
    public async Task GetDashboardStats_TopSellingProducts_ReturnsCorrectList()
    {
        var expectedStats = new DashboardStatsResponse
        {
            TopSellingProducts =
                new List<TopSellingProductResponse>
                {
                    new() { ProductName = "Xe Vision", QuantitySold = 50 },
                    new() { ProductName = "Xe Janus", QuantitySold = 30 }
                }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);
        var result = await _controller.GetDashboardStatsAsync(CancellationToken.None).ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualStats = okResult.Value.Should().BeAssignableTo<DashboardStatsResponse>().Subject;
        actualStats.TopSellingProducts.Should().HaveCount(2);
        actualStats.TopSellingProducts[0].ProductName.Should().Be("Xe Vision");
        actualStats.TopSellingProducts[0].QuantitySold.Should().Be(50);
    }

    [Fact(DisplayName = "STAT_110 - Controller - GetAdminDashboardOverviewAsync thành công với tham số mặc định")]
    public async Task GetAdminDashboardOverview_DefaultParams_ReturnsOkWithData()
    {
        var expectedOverview = new AdminDashboardOverviewResponse
        {
            Summary = new DashboardStatsResponse { TodayRevenue = 20000000, MonthlyRevenue = 200000000 },
            DailyRevenue = [new DailyRevenueResponse { ReportDay = DateOnly.FromDateTime(DateTime.Today), TotalRevenue = 20000000 }],
            OrderStatusDistribution = [new OrderStatusCountResponse { StatusName = OrderStatus.Completed, OrderCount = 10 }],
            RecentOrders = [new RecentOrderResponse { Id = 1, OrderCode = "ORD-001", TotalAmount = 5000000 }],
            TopStaff = [new StaffPerformanceResponse { EmployeeName = "Nguyen Van A", TotalSales = 50000000 }],
            RecentTransactions = [new TransactionLogResponse { CustomerName = "Le Van C", Amount = 5000000 }]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAdminDashboardOverviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOverview);

        var result = await _controller.GetAdminDashboardOverviewAsync(null, null, CancellationToken.None).ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualOverview = okResult.Value.Should().BeAssignableTo<AdminDashboardOverviewResponse>().Subject;

        actualOverview.Summary.TodayRevenue.Should().Be(20000000);
        actualOverview.DailyRevenue.Should().HaveCount(1);
        actualOverview.RecentOrders.Should().HaveCount(1);
        actualOverview.TopStaff.Should().HaveCount(1);
        actualOverview.RecentTransactions.Should().HaveCount(1);
    }

    [Fact(DisplayName = "STAT_111 - Controller - GetAdminDashboardOverviewAsync truyền đúng StartDate và EndDate theo Tháng")]
    public async Task GetAdminDashboardOverview_MonthParams_PassesCorrectDatesToQuery()
    {
        var startMonth = new DateTime(2026, 8, 1);
        var endMonth = new DateTime(2026, 8, 31);
        var expectedOverview = new AdminDashboardOverviewResponse
        {
            Summary = new DashboardStatsResponse { MonthlyRevenue = 500000000, PeriodRevenue = 500000000 }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetAdminDashboardOverviewQuery>(q => q.StartDate == startMonth && q.EndDate == endMonth),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOverview);

        var result = await _controller.GetAdminDashboardOverviewAsync(startMonth, endMonth, CancellationToken.None).ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualOverview = okResult.Value.Should().BeAssignableTo<AdminDashboardOverviewResponse>().Subject;

        actualOverview.Summary.PeriodRevenue.Should().Be(500000000);
    }

    [Fact(DisplayName = "STAT_112 - Controller - GetAdminDashboardOverviewAsync truyền đúng StartDate và EndDate theo Năm")]
    public async Task GetAdminDashboardOverview_YearParams_PassesCorrectDatesToQuery()
    {
        var startYear = new DateTime(2026, 1, 1);
        var endYear = new DateTime(2026, 12, 31);
        var expectedOverview = new AdminDashboardOverviewResponse
        {
            Summary = new DashboardStatsResponse { YearlyRevenue = 5000000000, PeriodRevenue = 5000000000 }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetAdminDashboardOverviewQuery>(q => q.StartDate == startYear && q.EndDate == endYear),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOverview);

        var result = await _controller.GetAdminDashboardOverviewAsync(startYear, endYear, CancellationToken.None).ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actualOverview = okResult.Value.Should().BeAssignableTo<AdminDashboardOverviewResponse>().Subject;

        actualOverview.Summary.YearlyRevenue.Should().Be(5000000000);
    }

#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
