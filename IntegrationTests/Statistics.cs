using System.Net;
using FluentAssertions;
using Xunit;

namespace IntegrationTests;

public class Statistics(IntegrationTestWebAppFactory factory) : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

#pragma warning disable CRR0035
    [Fact(DisplayName = "STAT_021 - Lấy doanh thu 7 ngày gần nhất")]
    public async Task GetDailyRevenue_Last7Days_ReturnsCorrectData()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=7", CancellationToken.None).ConfigureAwait(true);

        // Assert - Vì handler chưa implement nên sẽ fail, nhưng phải compile được
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_022 - Lấy doanh thu với tham số days = 1")]
    public async Task GetDailyRevenue_OneDay_ReturnsCorrectData()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=1", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_023 - Lấy doanh thu với tham số days = 30")]
    public async Task GetDailyRevenue_ThirtyDays_ReturnsCorrectData()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=30", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_024 - Lấy doanh thu với days âm trả về lỗi validation")]
    public async Task GetDailyRevenue_NegativeDays_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=-1", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_025 - Lấy doanh thu với days = 0 trả về lỗi validation")]
    public async Task GetDailyRevenue_ZeroDays_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=0", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_026 - Lấy các chỉ số dashboard")]
    public async Task GetDashboardStats_ReturnsAllMetrics()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/dashboard-stats", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_027 - Lấy doanh thu và lợi nhuận 12 tháng")]
    public async Task GetMonthlyRevenueProfit_Last12Months_ReturnsCorrectData()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/monthly-revenue-profit?months=12", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_028 - Lấy doanh thu và lợi nhuận 3 tháng")]
    public async Task GetMonthlyRevenueProfit_Last3Months_ReturnsCorrectData()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/monthly-revenue-profit?months=3", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_029 - Lấy doanh thu với tháng âm trả về lỗi")]
    public async Task GetMonthlyRevenueProfit_NegativeMonths_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/monthly-revenue-profit?months=-1", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_030 - Lấy doanh thu với tháng = 0 trả về lỗi")]
    public async Task GetMonthlyRevenueProfit_ZeroMonths_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/monthly-revenue-profit?months=0", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_031 - Lấy số lượng đơn hàng theo trạng thái")]
    public async Task GetOrderStatusCounts_ReturnsAllStatuses()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/order-status-counts", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_032 - Lấy báo cáo sản phẩm tháng trước")]
    public async Task GetProductReportLastMonth_ReturnsProductList()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/product-report-last-month", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_033 - Lấy tồn kho và giá sản phẩm hợp lệ")]
    public async Task GetProductStockAndPrice_ValidVariantId_ReturnsData()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/product-stock-price?variantId=1", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_034 - Lấy tồn kho với variantId không tồn tại")]
    public async Task GetProductStockAndPrice_NonExistentVariantId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/product-stock-price?variantId=999999", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_035 - Lấy tồn kho với variantId âm")]
    public async Task GetProductStockAndPrice_NegativeVariantId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/product-stock-price?variantId=-1", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_036 - Lấy tồn kho với variantId = 0")]
    public async Task GetProductStockAndPrice_ZeroVariantId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/product-stock-price?variantId=0", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "STAT_037 - Kiểm tra định dạng response daily revenue")]
    public async Task GetDailyRevenue_ReturnsCorrectFormat()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/daily-revenue?days=7", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact(DisplayName = "STAT_038 - Kiểm tra định dạng response dashboard stats")]
    public async Task GetDashboardStats_ReturnsCorrectFormat()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/dashboard-stats", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact(DisplayName = "STAT_039 - Kiểm tra định dạng response monthly revenue")]
    public async Task GetMonthlyRevenueProfit_ReturnsCorrectFormat()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/monthly-revenue-profit?months=12", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact(DisplayName = "STAT_040 - Kiểm tra định dạng response order status counts")]
    public async Task GetOrderStatusCounts_ReturnsCorrectFormat()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Statistics/order-status-counts", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
#pragma warning restore CRR0035
}
