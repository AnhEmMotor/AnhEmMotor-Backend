using Application.ApiContracts.Statistical.Responses;
using Application.Features.Statistical.Queries.GetDailyRevenue;
using Application.Features.Statistical.Queries.GetDashboardStats;
using Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;
using Application.Features.Statistical.Queries.GetOrderStatusCounts;
using Application.Features.Statistical.Queries.GetProductReportLastMonth;
using Application.Features.Statistical.Queries.GetProductStockAndPrice;
using Application.Interfaces.Repositories.Statistical;
using Domain.Constants.Order;
using FluentAssertions;
using Moq;
using Xunit;

namespace UnitTests;

public class Statistics
{
    private readonly Mock<IStatisticalReadRepository> _repositoryMock;

    public Statistics()
    {
        _repositoryMock = new Mock<IStatisticalReadRepository>();
    }

    [Fact(DisplayName = "STAT_041 - Unit - GetDailyRevenueQueryHandler xử lý days hợp lệ")]
    public async Task Handle_ValidDays7_Returns7DaysData()
    {
        // Arrange
        var query = new GetDailyRevenueQuery(7);
        var expectedData = new List<DailyRevenueResponse>();
        for (int i = 0; i < 7; i++)
        {
            expectedData.Add(new DailyRevenueResponse
            {
                ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-i)),
                TotalRevenue = (i + 1) * 1000000
            });
        }

        _repositoryMock.Setup(r => r.GetDailyRevenueAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetDailyRevenueQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().HaveCount(7);
    }

    [Fact(DisplayName = "STAT_042 - Unit - GetDailyRevenueQueryHandler với days = 1")]
    public async Task Handle_Days1_Returns1DayData()
    {
        // Arrange
        var query = new GetDailyRevenueQuery(1);
        var expectedData = new List<DailyRevenueResponse>
        {
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now), TotalRevenue = 5000000 }
        };

        _repositoryMock.Setup(r => r.GetDailyRevenueAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetDailyRevenueQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(1);
        result.Value.First().TotalRevenue.Should().Be(5000000);
    }

    [Fact(DisplayName = "STAT_043 - Unit - GetDailyRevenueQueryHandler tính tổng doanh thu đúng")]
    public async Task Handle_Days3_CalculatesTotalRevenueCorrectly()
    {
        // Arrange
        var query = new GetDailyRevenueQuery(3);
        var expectedData = new List<DailyRevenueResponse>
        {
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)), TotalRevenue = 1000000 },
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), TotalRevenue = 2000000 },
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now), TotalRevenue = 1500000 }
        };

        _repositoryMock.Setup(r => r.GetDailyRevenueAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetDailyRevenueQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var total = result.Value.Sum(x => x.TotalRevenue);
        total.Should().Be(4500000m);
    }

    [Fact(DisplayName = "STAT_044 - Unit - GetDailyRevenueQueryHandler với doanh thu = 0")]
    public async Task Handle_ZeroRevenue_Returns5DaysWithZero()
    {
        // Arrange
        var query = new GetDailyRevenueQuery(5);
        var expectedData = new List<DailyRevenueResponse>();
        for (int i = 0; i < 5; i++)
        {
            expectedData.Add(new DailyRevenueResponse
            {
                ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-i)),
                TotalRevenue = 0
            });
        }

        _repositoryMock.Setup(r => r.GetDailyRevenueAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetDailyRevenueQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(5);
        result.Value.All(x => x.TotalRevenue == 0).Should().BeTrue();
    }

    [Fact(DisplayName = "STAT_045 - Unit - GetDailyRevenueQueryHandler với số thập phân")]
    public async Task Handle_DecimalRevenue_PreservesDecimalPlaces()
    {
        // Arrange
        var query = new GetDailyRevenueQuery(2);
        var expectedData = new List<DailyRevenueResponse>
        {
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), TotalRevenue = 1234567.89m },
            new() { ReportDay = DateOnly.FromDateTime(DateTime.Now), TotalRevenue = 9876543.21m }
        };

        _repositoryMock.Setup(r => r.GetDailyRevenueAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetDailyRevenueQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().TotalRevenue.Should().Be(1234567.89m);
        result.Value.Last().TotalRevenue.Should().Be(9876543.21m);
    }

    [Fact(DisplayName = "STAT_046 - Unit - GetDailyRevenueQueryHandler gọi repository đúng tham số")]
    public async Task Handle_Days10_CallsRepositoryWithCorrectParameter()
    {
        // Arrange
        var query = new GetDailyRevenueQuery(10);
        _repositoryMock.Setup(r => r.GetDailyRevenueAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetDailyRevenueQueryHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetDailyRevenueAsync(10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "STAT_047 - Unit - GetDailyRevenueQueryHandler với dữ liệu null")]
    public async Task Handle_NullData_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetDailyRevenueQuery(7);
        _repositoryMock.Setup(r => r.GetDailyRevenueAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<DailyRevenueResponse>)null!);

        var handler = new GetDailyRevenueQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "STAT_048 - Unit - GetDailyRevenueQueryHandler format ngày đúng")]
    public async Task Handle_Days3_ReturnsCorrectDateFormat()
    {
        // Arrange
        var query = new GetDailyRevenueQuery(3);
        var date1 = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));
        var date2 = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var date3 = DateOnly.FromDateTime(DateTime.Now);

        var expectedData = new List<DailyRevenueResponse>
        {
            new() { ReportDay = date1, TotalRevenue = 1000000 },
            new() { ReportDay = date2, TotalRevenue = 2000000 },
            new() { ReportDay = date3, TotalRevenue = 3000000 }
        };

        _repositoryMock.Setup(r => r.GetDailyRevenueAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetDailyRevenueQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(3);
        result.Value.First().ReportDay.Should().Be(date1);
        result.Value.Last().ReportDay.Should().Be(date3);
    }

    [Fact(DisplayName = "STAT_049 - Unit - GetDashboardStatsQueryHandler tính lastMonthRevenue")]
    public async Task Handle_DashboardStats_ReturnsCorrectLastMonthRevenue()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        var expectedStats = new DashboardStatsResponse
        {
            LastMonthRevenue = 50000000,
            LastMonthProfit = 15000000,
            PendingOrdersCount = 10,
            NewCustomersCount = 25
        };

        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        var handler = new GetDashboardStatsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value!.LastMonthRevenue.Should().Be(50000000);
    }

    [Fact(DisplayName = "STAT_050 - Unit - GetDashboardStatsQueryHandler tính lastMonthProfit")]
    public async Task Handle_DashboardStats_CalculatesLastMonthProfit()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        var expectedStats = new DashboardStatsResponse
        {
            LastMonthRevenue = 30000000,
            LastMonthProfit = 12000000, // 30M revenue - 18M cost
            PendingOrdersCount = 5,
            NewCustomersCount = 10
        };

        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        var handler = new GetDashboardStatsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.LastMonthProfit.Should().Be(12000000);
    }

    [Fact(DisplayName = "STAT_051 - Unit - GetDashboardStatsQueryHandler tính pendingOrderCount")]
    public async Task Handle_DashboardStats_ReturnsCorrectPendingOrderCount()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        var expectedStats = new DashboardStatsResponse
        {
            LastMonthRevenue = 20000000,
            LastMonthProfit = 8000000,
            PendingOrdersCount = 15,
            NewCustomersCount = 20
        };

        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        var handler = new GetDashboardStatsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.PendingOrdersCount.Should().Be(15);
    }

    [Fact(DisplayName = "STAT_052 - Unit - GetDashboardStatsQueryHandler tính newUserCount")]
    public async Task Handle_DashboardStats_ReturnsCorrectNewUserCount()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        var expectedStats = new DashboardStatsResponse
        {
            LastMonthRevenue = 40000000,
            LastMonthProfit = 16000000,
            PendingOrdersCount = 8,
            NewCustomersCount = 25
        };

        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        var handler = new GetDashboardStatsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.NewCustomersCount.Should().Be(25);
    }

    [Fact(DisplayName = "STAT_053 - Unit - GetDashboardStatsQueryHandler với lợi nhuận âm")]
    public async Task Handle_DashboardStats_NegativeProfit_ReturnsNegativeValue()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        var expectedStats = new DashboardStatsResponse
        {
            LastMonthRevenue = 10000000,
            LastMonthProfit = -5000000, // Loss
            PendingOrdersCount = 3,
            NewCustomersCount = 5
        };

        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        var handler = new GetDashboardStatsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.LastMonthProfit.Should().Be(-5000000);
    }

    [Fact(DisplayName = "STAT_054 - Unit - GetDashboardStatsQueryHandler với tất cả = 0")]
    public async Task Handle_DashboardStats_NoData_ReturnsAllZeros()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        var expectedStats = new DashboardStatsResponse
        {
            LastMonthRevenue = 0,
            LastMonthProfit = 0,
            PendingOrdersCount = 0,
            NewCustomersCount = 0
        };

        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        var handler = new GetDashboardStatsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.LastMonthRevenue.Should().Be(0);
        result.Value.LastMonthProfit.Should().Be(0);
        result.Value.PendingOrdersCount.Should().Be(0);
        result.Value.NewCustomersCount.Should().Be(0);
    }

    [Fact(DisplayName = "STAT_055 - Unit - GetDashboardStatsQueryHandler gọi nhiều repository methods")]
    public async Task Handle_DashboardStats_CallsRepositoryOnce()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        _repositoryMock.Setup(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardStatsResponse());

        var handler = new GetDashboardStatsQueryHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetDashboardStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "STAT_056 - Unit - GetMonthlyRevenueProfitQueryHandler với months = 12")]
    public async Task Handle_Months12_Returns12MonthsData()
    {
        // Arrange
        var query = new GetMonthlyRevenueProfitQuery(12);
        var expectedData = new List<MonthlyRevenueProfitResponse>();
        for (int i = 0; i < 12; i++)
        {
            expectedData.Add(new MonthlyRevenueProfitResponse
            {
                ReportMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-i)),
                TotalRevenue = (i + 1) * 5000000,
                TotalProfit = (i + 1) * 1500000
            });
        }

        _repositoryMock.Setup(r => r.GetMonthlyRevenueProfitAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetMonthlyRevenueProfitQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(12);
    }

    [Fact(DisplayName = "STAT_057 - Unit - GetMonthlyRevenueProfitQueryHandler với months = 1")]
    public async Task Handle_Months1_Returns1MonthData()
    {
        // Arrange
        var query = new GetMonthlyRevenueProfitQuery(1);
        var expectedData = new List<MonthlyRevenueProfitResponse>
        {
            new()
            {
                ReportMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
                TotalRevenue = 10000000,
                TotalProfit = 3000000
            }
        };

        _repositoryMock.Setup(r => r.GetMonthlyRevenueProfitAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetMonthlyRevenueProfitQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(1);
        result.Value.First().TotalRevenue.Should().Be(10000000);
    }

    [Fact(DisplayName = "STAT_058 - Unit - GetMonthlyRevenueProfitQueryHandler tính profit chính xác")]
    public async Task Handle_Months3_CalculatesProfitCorrectly()
    {
        // Arrange
        var query = new GetMonthlyRevenueProfitQuery(3);
        var expectedData = new List<MonthlyRevenueProfitResponse>
        {
            new() { ReportMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)), TotalRevenue = 10000000, TotalProfit = 4000000 },
            new() { ReportMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)), TotalRevenue = 8000000, TotalProfit = 3000000 },
            new() { ReportMonth = DateOnly.FromDateTime(DateTime.Now), TotalRevenue = 12000000, TotalProfit = 5000000 }
        };

        _repositoryMock.Setup(r => r.GetMonthlyRevenueProfitAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetMonthlyRevenueProfitQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.ElementAt(0).TotalProfit.Should().Be(4000000);
        result.Value.ElementAt(1).TotalProfit.Should().Be(3000000);
        result.Value.ElementAt(2).TotalProfit.Should().Be(5000000);
    }

    [Fact(DisplayName = "STAT_059 - Unit - GetMonthlyRevenueProfitQueryHandler với tháng không có doanh thu")]
    public async Task Handle_Months2_MonthWithNoRevenue_ReturnsZeroProfit()
    {
        // Arrange
        var query = new GetMonthlyRevenueProfitQuery(2);
        var expectedData = new List<MonthlyRevenueProfitResponse>
        {
            new() { ReportMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)), TotalRevenue = 0, TotalProfit = 0 },
            new() { ReportMonth = DateOnly.FromDateTime(DateTime.Now), TotalRevenue = 5000000, TotalProfit = 2000000 }
        };

        _repositoryMock.Setup(r => r.GetMonthlyRevenueProfitAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetMonthlyRevenueProfitQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().TotalProfit.Should().Be(0);
        result.Value.Last().TotalProfit.Should().Be(2000000);
    }

    [Fact(DisplayName = "STAT_060 - Unit - GetMonthlyRevenueProfitQueryHandler format tháng đúng")]
    public async Task Handle_Months6_ReturnsCorrectMonthFormat()
    {
        // Arrange
        var query = new GetMonthlyRevenueProfitQuery(6);
        var expectedData = new List<MonthlyRevenueProfitResponse>();
        for (int i = 0; i < 6; i++)
        {
            var month = DateTime.Now.AddMonths(-i);
            expectedData.Add(new MonthlyRevenueProfitResponse
            {
                ReportMonth = new DateOnly(month.Year, month.Month, 1),
                TotalRevenue = 1000000,
                TotalProfit = 300000
            });
        }

        _repositoryMock.Setup(r => r.GetMonthlyRevenueProfitAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetMonthlyRevenueProfitQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(6);
        result.Value.All(x => x.ReportMonth.Day == 1).Should().BeTrue();
    }

    [Fact(DisplayName = "STAT_061 - Unit - GetMonthlyRevenueProfitQueryHandler sắp xếp theo thứ tự")]
    public async Task Handle_Months5_ReturnsSortedData()
    {
        // Arrange
        var query = new GetMonthlyRevenueProfitQuery(5);
        var expectedData = new List<MonthlyRevenueProfitResponse>();
        for (int i = 4; i >= 0; i--)
        {
            expectedData.Add(new MonthlyRevenueProfitResponse
            {
                ReportMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-i)),
                TotalRevenue = (i + 1) * 1000000,
                TotalProfit = (i + 1) * 300000
            });
        }

        _repositoryMock.Setup(r => r.GetMonthlyRevenueProfitAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetMonthlyRevenueProfitQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(5);
        result.Value.Should().BeInAscendingOrder(x => x.ReportMonth);
    }

    [Fact(DisplayName = "STAT_062 - Unit - GetMonthlyRevenueProfitQueryHandler với số thập phân")]
    public async Task Handle_Months2_PreservesDecimalInProfit()
    {
        // Arrange
        var query = new GetMonthlyRevenueProfitQuery(2);
        var expectedData = new List<MonthlyRevenueProfitResponse>
        {
            new() { ReportMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)), TotalRevenue = 1234567.89m, TotalProfit = 1000000.77m },
            new() { ReportMonth = DateOnly.FromDateTime(DateTime.Now), TotalRevenue = 9876543.21m, TotalProfit = 7000000.10m }
        };

        _repositoryMock.Setup(r => r.GetMonthlyRevenueProfitAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetMonthlyRevenueProfitQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().TotalProfit.Should().Be(1000000.77m);
        result.Value.Last().TotalProfit.Should().Be(7000000.10m);
    }

    [Fact(DisplayName = "STAT_063 - Unit - GetMonthlyRevenueProfitQueryHandler gọi repository đúng")]
    public async Task Handle_Months8_CallsRepositoryWithCorrectParameter()
    {
        // Arrange
        var query = new GetMonthlyRevenueProfitQuery(8);
        _repositoryMock.Setup(r => r.GetMonthlyRevenueProfitAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetMonthlyRevenueProfitQueryHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetMonthlyRevenueProfitAsync(8, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "STAT_064 - Unit - GetOrderStatusCountsQueryHandler đếm Pending")]
    public async Task Handle_OrderStatusCounts_ReturnsPendingCount()
    {
        // Arrange
        var query = new GetOrderStatusCountsQuery();
        var expectedData = new List<OrderStatusCountResponse>
        {
            new() { StatusName = OrderStatus.Pending, OrderCount = 10 },
            new() { StatusName = OrderStatus.Completed, OrderCount = 20 }
        };

        _repositoryMock.Setup(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetOrderStatusCountsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var pendingCount = result.Value.First(x => x.StatusName == OrderStatus.Pending);
        pendingCount.OrderCount.Should().Be(10);
    }

    [Fact(DisplayName = "STAT_065 - Unit - GetOrderStatusCountsQueryHandler đếm Processing")]
    public async Task Handle_OrderStatusCounts_ReturnsProcessingCount()
    {
        // Arrange
        var query = new GetOrderStatusCountsQuery();
        var expectedData = new List<OrderStatusCountResponse>
        {
            new() { StatusName = OrderStatus.PaidProcessing, OrderCount = 15 }
        };

        _repositoryMock.Setup(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetOrderStatusCountsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().OrderCount.Should().Be(15);
    }

    [Fact(DisplayName = "STAT_066 - Unit - GetOrderStatusCountsQueryHandler đếm Delivering")]
    public async Task Handle_OrderStatusCounts_ReturnsDeliveringCount()
    {
        // Arrange
        var query = new GetOrderStatusCountsQuery();
        var expectedData = new List<OrderStatusCountResponse>
        {
            new() { StatusName = OrderStatus.Delivering, OrderCount = 8 }
        };

        _repositoryMock.Setup(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetOrderStatusCountsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().OrderCount.Should().Be(8);
    }

    [Fact(DisplayName = "STAT_067 - Unit - GetOrderStatusCountsQueryHandler đếm WaitingPickup")]
    public async Task Handle_OrderStatusCounts_ReturnsWaitingPickupCount()
    {
        // Arrange
        var query = new GetOrderStatusCountsQuery();
        var expectedData = new List<OrderStatusCountResponse>
        {
            new() { StatusName = OrderStatus.WaitingPickup, OrderCount = 5 }
        };

        _repositoryMock.Setup(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetOrderStatusCountsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().OrderCount.Should().Be(5);
    }

    [Fact(DisplayName = "STAT_068 - Unit - GetOrderStatusCountsQueryHandler đếm Completed")]
    public async Task Handle_OrderStatusCounts_ReturnsCompletedCount()
    {
        // Arrange
        var query = new GetOrderStatusCountsQuery();
        var expectedData = new List<OrderStatusCountResponse>
        {
            new() { StatusName = OrderStatus.Completed, OrderCount = 50 }
        };

        _repositoryMock.Setup(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetOrderStatusCountsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().OrderCount.Should().Be(50);
    }

    [Fact(DisplayName = "STAT_069 - Unit - GetOrderStatusCountsQueryHandler đếm Cancelled")]
    public async Task Handle_OrderStatusCounts_ReturnsCancelledCount()
    {
        // Arrange
        var query = new GetOrderStatusCountsQuery();
        var expectedData = new List<OrderStatusCountResponse>
        {
            new() { StatusName = OrderStatus.Cancelled, OrderCount = 3 }
        };

        _repositoryMock.Setup(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetOrderStatusCountsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().OrderCount.Should().Be(3);
    }

    [Fact(DisplayName = "STAT_070 - Unit - GetOrderStatusCountsQueryHandler tất cả = 0")]
    public async Task Handle_OrderStatusCounts_NoOrders_ReturnsAllZeros()
    {
        // Arrange
        var query = new GetOrderStatusCountsQuery();
        var expectedData = new List<OrderStatusCountResponse>
        {
            new() { StatusName = OrderStatus.Pending, OrderCount = 0 },
            new() { StatusName = OrderStatus.Completed, OrderCount = 0 },
            new() { StatusName = OrderStatus.Cancelled, OrderCount = 0 }
        };

        _repositoryMock.Setup(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetOrderStatusCountsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.All(x => x.OrderCount == 0).Should().BeTrue();
    }

    [Fact(DisplayName = "STAT_071 - Unit - GetOrderStatusCountsQueryHandler tổng số đúng")]
    public async Task Handle_OrderStatusCounts_CalculatesTotalCorrectly()
    {
        // Arrange
        var query = new GetOrderStatusCountsQuery();
        var expectedData = new List<OrderStatusCountResponse>
        {
            new() { StatusName = OrderStatus.Pending, OrderCount = 5 },
            new() { StatusName = OrderStatus.PaidProcessing, OrderCount = 10 },
            new() { StatusName = OrderStatus.Completed, OrderCount = 15 }
        };

        _repositoryMock.Setup(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetOrderStatusCountsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var total = result.Value.Sum(x => x.OrderCount);
        total.Should().Be(30);
    }

    [Fact(DisplayName = "STAT_072 - Unit - GetOrderStatusCountsQueryHandler gọi repository")]
    public async Task Handle_OrderStatusCounts_CallsRepositoryOnce()
    {
        // Arrange
        var query = new GetOrderStatusCountsQuery();
        _repositoryMock.Setup(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetOrderStatusCountsQueryHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetOrderStatusCountsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "STAT_073 - Unit - GetProductReportLastMonthQueryHandler với nhiều sản phẩm")]
    public async Task Handle_ProductReport_Returns10Products()
    {
        // Arrange
        var query = new GetProductReportLastMonthQuery();
        var expectedData = new List<ProductReportResponse>();
        for (int i = 0; i < 10; i++)
        {
            expectedData.Add(new ProductReportResponse
            {
                ProductName = $"Product {i}",
                VariantId = i + 1,
                SoldLastMonth = (i + 1) * 5,
                StockQuantity = (i + 1) * 20
            });
        }

        _repositoryMock.Setup(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductReportLastMonthQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(10);
    }

    [Fact(DisplayName = "STAT_074 - Unit - GetProductReportLastMonthQueryHandler tính soldQuantity")]
    public async Task Handle_ProductReport_ReturnsSoldQuantity()
    {
        // Arrange
        var query = new GetProductReportLastMonthQuery();
        var expectedData = new List<ProductReportResponse>
        {
            new() { ProductName = "Product A", VariantId = 1, SoldLastMonth = 25, StockQuantity = 100 }
        };

        _repositoryMock.Setup(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductReportLastMonthQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().SoldLastMonth.Should().Be(25);
    }

    [Fact(DisplayName = "STAT_075 - Unit - GetProductReportLastMonthQueryHandler hiển thị stockQuantity")]
    public async Task Handle_ProductReport_ReturnsStockQuantity()
    {
        // Arrange
        var query = new GetProductReportLastMonthQuery();
        var expectedData = new List<ProductReportResponse>
        {
            new() { ProductName = "Product B", VariantId = 2, SoldLastMonth = 10, StockQuantity = 150 }
        };

        _repositoryMock.Setup(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductReportLastMonthQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().StockQuantity.Should().Be(150);
    }

    [Fact(DisplayName = "STAT_076 - Unit - GetProductReportLastMonthQueryHandler với sản phẩm chưa bán")]
    public async Task Handle_ProductReport_UnsoldProduct_ReturnsZeroSold()
    {
        // Arrange
        var query = new GetProductReportLastMonthQuery();
        var expectedData = new List<ProductReportResponse>
        {
            new() { ProductName = "Product C", VariantId = 3, SoldLastMonth = 0, StockQuantity = 200 }
        };

        _repositoryMock.Setup(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductReportLastMonthQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().SoldLastMonth.Should().Be(0);
        result.Value.First().StockQuantity.Should().Be(200);
    }

    [Fact(DisplayName = "STAT_077 - Unit - GetProductReportLastMonthQueryHandler với sản phẩm hết hàng")]
    public async Task Handle_ProductReport_OutOfStock_ReturnsZeroStock()
    {
        // Arrange
        var query = new GetProductReportLastMonthQuery();
        var expectedData = new List<ProductReportResponse>
        {
            new() { ProductName = "Product D", VariantId = 4, SoldLastMonth = 50, StockQuantity = 0 }
        };

        _repositoryMock.Setup(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductReportLastMonthQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().SoldLastMonth.Should().Be(50);
        result.Value.First().StockQuantity.Should().Be(0);
    }

    [Fact(DisplayName = "STAT_078 - Unit - GetProductReportLastMonthQueryHandler hiển thị tên sản phẩm")]
    public async Task Handle_ProductReport_ReturnsProductName()
    {
        // Arrange
        var query = new GetProductReportLastMonthQuery();
        var expectedData = new List<ProductReportResponse>
        {
            new() { ProductName = "Xe máy Honda Wave", VariantId = 5, SoldLastMonth = 15, StockQuantity = 30 }
        };

        _repositoryMock.Setup(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductReportLastMonthQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().ProductName.Should().Be("Xe máy Honda Wave");
    }

    [Fact(DisplayName = "STAT_079 - Unit - GetProductReportLastMonthQueryHandler bao gồm sản phẩm xóa")]
    public async Task Handle_ProductReport_IncludesDeletedProducts()
    {
        // Arrange
        var query = new GetProductReportLastMonthQuery();
        var expectedData = new List<ProductReportResponse>
        {
            new() { ProductName = "Deleted Product", VariantId = 6, SoldLastMonth = 10, StockQuantity = 0 },
            new() { ProductName = "Active Product", VariantId = 7, SoldLastMonth = 20, StockQuantity = 50 }
        };

        _repositoryMock.Setup(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductReportLastMonthQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(2);
        result.Value.Any(x => x.ProductName == "Deleted Product").Should().BeTrue();
    }

    [Fact(DisplayName = "STAT_080 - Unit - GetProductReportLastMonthQueryHandler sắp xếp theo số lượng bán")]
    public async Task Handle_ProductReport_SortsBySoldQuantityDescending()
    {
        // Arrange
        var query = new GetProductReportLastMonthQuery();
        var expectedData = new List<ProductReportResponse>
        {
            new() { ProductName = "Product A", VariantId = 1, SoldLastMonth = 50, StockQuantity = 100 },
            new() { ProductName = "Product B", VariantId = 2, SoldLastMonth = 30, StockQuantity = 150 },
            new() { ProductName = "Product C", VariantId = 3, SoldLastMonth = 40, StockQuantity = 120 }
        };

        _repositoryMock.Setup(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductReportLastMonthQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeInDescendingOrder(x => x.SoldLastMonth);
    }

    [Fact(DisplayName = "STAT_081 - Unit - GetProductReportLastMonthQueryHandler gọi repository")]
    public async Task Handle_ProductReport_CallsRepositoryOnce()
    {
        // Arrange
        var query = new GetProductReportLastMonthQuery();
        _repositoryMock.Setup(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetProductReportLastMonthQueryHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetProductReportLastMonthAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "STAT_082 - Unit - GetProductStockAndPriceQueryHandler với variant hợp lệ")]
    public async Task Handle_VariantId10_ReturnsPriceAndStock()
    {
        // Arrange
        var query = new GetProductStockAndPriceQuery(10);
        var expectedData = new ProductStockPriceResponse
        {
            UnitPrice = 2500000,
            StockQuantity = 50
        };

        _repositoryMock.Setup(r => r.GetProductStockAndPriceAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductStockAndPriceQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value!.UnitPrice.Should().Be(2500000);
        result.Value.StockQuantity.Should().Be(50);
    }

    [Fact(DisplayName = "STAT_083 - Unit - GetProductStockAndPriceQueryHandler với giá = 0")]
    public async Task Handle_VariantId5_ZeroPrice_ReturnsZeroPrice()
    {
        // Arrange
        var query = new GetProductStockAndPriceQuery(5);
        var expectedData = new ProductStockPriceResponse
        {
            UnitPrice = 0,
            StockQuantity = 100
        };

        _repositoryMock.Setup(r => r.GetProductStockAndPriceAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductStockAndPriceQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.UnitPrice.Should().Be(0);
        result.Value.StockQuantity.Should().Be(100);
    }

    [Fact(DisplayName = "STAT_084 - Unit - GetProductStockAndPriceQueryHandler với tồn kho = 0")]
    public async Task Handle_VariantId8_ZeroStock_ReturnsZeroStock()
    {
        // Arrange
        var query = new GetProductStockAndPriceQuery(8);
        var expectedData = new ProductStockPriceResponse
        {
            UnitPrice = 1000000,
            StockQuantity = 0
        };

        _repositoryMock.Setup(r => r.GetProductStockAndPriceAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductStockAndPriceQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.UnitPrice.Should().Be(1000000);
        result.Value.StockQuantity.Should().Be(0);
    }

    [Fact(DisplayName = "STAT_085 - Unit - GetProductStockAndPriceQueryHandler với giá thập phân")]
    public async Task Handle_VariantId12_DecimalPrice_PreservesDecimal()
    {
        // Arrange
        var query = new GetProductStockAndPriceQuery(12);
        var expectedData = new ProductStockPriceResponse
        {
            UnitPrice = 1234567.89m,
            StockQuantity = 25
        };

        _repositoryMock.Setup(r => r.GetProductStockAndPriceAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductStockAndPriceQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.UnitPrice.Should().Be(1234567.89m);
    }

    [Fact(DisplayName = "STAT_086 - Unit - GetProductStockAndPriceQueryHandler gọi repository đúng ID")]
    public async Task Handle_VariantId15_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var query = new GetProductStockAndPriceQuery(15);
        _repositoryMock.Setup(r => r.GetProductStockAndPriceAsync(15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductStockPriceResponse());

        var handler = new GetProductStockAndPriceQueryHandler(_repositoryMock.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetProductStockAndPriceAsync(15, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "STAT_087 - Unit - GetProductStockAndPriceQueryHandler với variant không tồn tại")]
    public async Task Handle_VariantId999_NonExistent_ReturnsNull()
    {
        // Arrange
        var query = new GetProductStockAndPriceQuery(999);
        _repositoryMock.Setup(r => r.GetProductStockAndPriceAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductStockPriceResponse?)null);

        var handler = new GetProductStockAndPriceQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "STAT_088 - Unit - GetProductStockAndPriceQueryHandler với variant đã xóa")]
    public async Task Handle_VariantId20_Deleted_ReturnsData()
    {
        // Arrange
        var query = new GetProductStockAndPriceQuery(20);
        var expectedData = new ProductStockPriceResponse
        {
            UnitPrice = 500000,
            StockQuantity = 0
        };

        _repositoryMock.Setup(r => r.GetProductStockAndPriceAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        var handler = new GetProductStockAndPriceQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value!.UnitPrice.Should().Be(500000);
    }
}
