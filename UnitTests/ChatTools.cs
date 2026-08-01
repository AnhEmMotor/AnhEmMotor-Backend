using Application.ApiContracts.Output.Responses;
using Application.ApiContracts.Statistical.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Queries.GetLowStockProductsForChat;
using Application.Features.ChatTools.Queries.GetOrderStatusForChat;
using Application.Features.ChatTools.Queries.GetProductStockForChat;
using Application.Features.ChatTools.Queries.GetSalesSummaryForChat;
using Application.Features.ChatTools.Queries.GetTopSellingForChat;
using Application.Features.ChatTools.Queries.SearchProductsForChat;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.Statistical;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.Services;
using Moq;
using Sieve.Models;
using System.Linq.Expressions;
using DomainBrand = Domain.Entities.Brand;
using DomainOutput = Domain.Entities.Output;
using DomainProduct = Domain.Entities.Product;
using DomainProductVariant = Domain.Entities.ProductVariant;

namespace UnitTests;

public class ChatTools
{
    private readonly Mock<IProductReadRepository> _productReadRepositoryMock;
    private readonly Mock<IStatisticalReadRepository> _statisticalReadRepositoryMock;
    private readonly Mock<IOutputReadRepository> _outputReadRepositoryMock;
    private readonly IServerDateProvider _dateProvider;

    public ChatTools()
    {
        _productReadRepositoryMock = new Mock<IProductReadRepository>();
        _statisticalReadRepositoryMock = new Mock<IStatisticalReadRepository>();
        _outputReadRepositoryMock = new Mock<IOutputReadRepository>();
        _dateProvider = new SystemServerDateProvider();
    }

    #pragma warning disable IDE0079
    #pragma warning disable CRR0035

    [Fact(DisplayName = "CHATTOOLS_101 - Unit - SearchProductsForChatQueryHandler trả danh sách sản phẩm")]
    public async Task Search_ValidKeyword_ReturnsMappedProducts()
    {
        var product = new DomainProduct
        {
            Id = 1,
            Name = "Wave Alpha",
            Brand = new DomainBrand { Id = 1, Name = "Honda" },
            ProductVariants = [new DomainProductVariant { Id = 10, Price = 20000000 }, new DomainProductVariant { Id = 11, Price = 22000000 }]
        };
        _productReadRepositoryMock.Setup(
            r => r.GetPagedProductsAsync(
                "Wave",
                It.IsAny<List<string>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                null,
                null,
                1,
                It.IsAny<int>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([product], 1, new List<FilterGroup>()));
        var handler = new SearchProductsForChatQueryHandler(_productReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new SearchProductsForChatQuery { Keyword = "Wave" }, CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].BrandName.Should().Be("Honda");
        result.Value.Items[0].PriceFrom.Should().Be(20000000);
        result.Value.Items[0].PriceTo.Should().Be(22000000);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact(DisplayName = "CHATTOOLS_110 - Unit - SearchProductsForChatQueryHandler tìm được sản phẩm khi search LIKE thường không ra kết quả do lệch dấu tiếng Việt")]
    public async Task Search_KeywordCoDauNhungTenSanPhamThieuDau_VanTimDuocQuaFallback()
    {
        _productReadRepositoryMock.Setup(
            r => r.GetPagedProductsAsync(
                "Nhông sên đĩa",
                It.IsAny<List<string>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                null,
                null,
                1,
                It.IsAny<int>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0, new List<FilterGroup>()));

        var product = new DomainProduct { Id = 5, Name = "Nhông sên dĩa DID chính hãng", ProductVariants = [] };
        _productReadRepositoryMock.Setup(
            r => r.GetPagedProductsAsync(
                null,
                It.IsAny<List<string>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                null,
                null,
                1,
                It.IsAny<int>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([product], 1, new List<FilterGroup>()));

        var handler = new SearchProductsForChatQueryHandler(_productReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new SearchProductsForChatQuery { Keyword = "Nhông sên đĩa" }, CancellationToken.None)
            .ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
        result.Value.Items[0].ProductName.Should().Be("Nhông sên dĩa DID chính hãng");
    }

    [Fact(DisplayName = "CHATTOOLS_111 - Unit - SearchProductsForChatQueryHandler tìm được sản phẩm khi tên sản phẩm thiếu dấu hoàn toàn")]
    public async Task Search_KeywordCoDauNhungTenSanPhamKhongCoDau_VanTimDuocQuaFallback()
    {
        _productReadRepositoryMock.Setup(
            r => r.GetPagedProductsAsync(
                "Phuộc nhún",
                It.IsAny<List<string>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                null,
                null,
                1,
                It.IsAny<int>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0, new List<FilterGroup>()));

        var product = new DomainProduct { Id = 6, Name = "Phuoc nhun truoc sau Yamaha", ProductVariants = [] };
        _productReadRepositoryMock.Setup(
            r => r.GetPagedProductsAsync(
                null,
                It.IsAny<List<string>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                null,
                null,
                1,
                It.IsAny<int>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([product], 1, new List<FilterGroup>()));

        var handler = new SearchProductsForChatQueryHandler(_productReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new SearchProductsForChatQuery { Keyword = "Phuộc nhún" }, CancellationToken.None)
            .ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
        result.Value.Items[0].ProductName.Should().Be("Phuoc nhun truoc sau Yamaha");
    }

    [Fact(DisplayName = "CHATTOOLS_112 - Unit - SearchProductsForChatQueryHandler tìm được sản phẩm khi lệch vị trí dấu tiếng Việt")]
    public async Task Search_KeywordLechViTriDau_VanTimDuocQuaFallback()
    {
        _productReadRepositoryMock.Setup(
            r => r.GetPagedProductsAsync(
                "vành đúc",
                It.IsAny<List<string>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                null,
                null,
                1,
                It.IsAny<int>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0, new List<FilterGroup>()));

        var product = new DomainProduct { Id = 7, Name = "Vành đuc hợp kim 17 inch", ProductVariants = [] };
        _productReadRepositoryMock.Setup(
            r => r.GetPagedProductsAsync(
                null,
                It.IsAny<List<string>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                It.IsAny<List<int>>(),
                null,
                null,
                1,
                It.IsAny<int>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([product], 1, new List<FilterGroup>()));

        var handler = new SearchProductsForChatQueryHandler(_productReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new SearchProductsForChatQuery { Keyword = "vành đúc" }, CancellationToken.None)
            .ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
        result.Value.Items[0].ProductName.Should().Be("Vành đuc hợp kim 17 inch");
    }

    [Fact(DisplayName = "CHATTOOLS_102 - Unit - GetProductStockForChatQueryHandler trả tồn kho theo variant")]
    public async Task GetProductStock_ValidProduct_ReturnsVariantStock()
    {
        var product = new DomainProduct { Id = 1, Name = "Vision", ProductVariants = [new DomainProductVariant { Id = 10, Price = 30000000 }] };
        _productReadRepositoryMock.Setup(r => r.GetByIdWithVariantsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync([product]);
        _statisticalReadRepositoryMock.Setup(r => r.GetProductStockAndPriceAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductStockPriceResponse { UnitPrice = 30000000, StockQuantity = 15 });
        var handler = new GetProductStockForChatQueryHandler(_productReadRepositoryMock.Object, _statisticalReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new GetProductStockForChatQuery { ProductId = 1 }, CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
        result.Value.Items[0].StockQuantity.Should().Be(15);
    }

    [Fact(DisplayName = "CHATTOOLS_103 - Unit - GetProductStockForChatQueryHandler với sản phẩm không tồn tại")]
    public async Task GetProductStock_ProductNotFound_ReturnsFailure()
    {
        _productReadRepositoryMock.Setup(r => r.GetByIdWithVariantsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync([]);
        var handler = new GetProductStockForChatQueryHandler(_productReadRepositoryMock.Object, _statisticalReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new GetProductStockForChatQuery { ProductId = 999 }, CancellationToken.None)
            .ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "CHATTOOLS_104 - Unit - GetLowStockProductsForChatQueryHandler chỉ trả sản phẩm sắp/hết hàng")]
    public async Task GetLowStock_FiltersOutInStockProducts()
    {
        var performance = new List<ProductPerformanceTableResponse>
        {
            new() { ProductName = "A", StockQuantity = 0, Status = "Hết hàng" },
            new() { ProductName = "B", StockQuantity = 3, Status = "Sắp hết" },
            new() { ProductName = "C", StockQuantity = 50, Status = "Còn hàng" }
        };
        _statisticalReadRepositoryMock.Setup(
            r => r.GetProductPerformanceTableAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(performance);
        var handler = new GetLowStockProductsForChatQueryHandler(_statisticalReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new GetLowStockProductsForChatQuery(), CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().OnlyContain(i => i.Status != "Còn hàng");
    }

    [Fact(DisplayName = "CHATTOOLS_105 - Unit - GetOrderStatusForChatQueryHandler trả trạng thái đơn hàng")]
    public async Task GetOrderStatus_ValidOrder_ReturnsStatus()
    {
        var order = new DomainOutput { Id = 123, StatusId = "completed", CustomerName = "Nguyễn Văn A" };
        _outputReadRepositoryMock.Setup(
            r => r.GetPagedAsync<OutputItemResponse>(
                It.IsAny<SieveModel>(),
                It.IsAny<Domain.Constants.DataFetchMode>(),
                It.IsAny<Expression<Func<DomainOutput, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<OutputItemResponse>([new OutputItemResponse { Id = 123 }], 1, 1, 5));
        _outputReadRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(123, It.IsAny<CancellationToken>(), It.IsAny<Domain.Constants.DataFetchMode>()))
            .ReturnsAsync(order);
        var handler = new GetOrderStatusForChatQueryHandler(_outputReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new GetOrderStatusForChatQuery { Keyword = "Nguyễn Văn A" }, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
        result.Value.Items[0].StatusId.Should().Be("completed");
        result.Value.Items[0].CustomerName.Should().Be("Nguyễn Văn A");
    }

    [Fact(DisplayName = "CHATTOOLS_106 - Unit - GetOrderStatusForChatQueryHandler không tìm thấy đơn hàng khớp keyword")]
    public async Task GetOrderStatus_NoMatch_ReturnsEmptyItems()
    {
        _outputReadRepositoryMock.Setup(
            r => r.GetPagedAsync<OutputItemResponse>(
                It.IsAny<SieveModel>(),
                It.IsAny<Domain.Constants.DataFetchMode>(),
                It.IsAny<Expression<Func<DomainOutput, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<OutputItemResponse>([], 0, 1, 5));
        var handler = new GetOrderStatusForChatQueryHandler(_outputReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new GetOrderStatusForChatQuery { Keyword = "Không tồn tại" }, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "CHATTOOLS_107 - Unit - GetSalesSummaryForChatQueryHandler giới hạn số bản ghi trả về")]
    public async Task GetSalesSummary_LimitsRecordsReturned()
    {
        var daily = Enumerable.Range(0, 15)
            .Select(i => new DailyRevenueResponse { ReportDay = DateOnly.FromDateTime(DateTime.Now.AddDays(-i)), TotalRevenue = i * 1000000 })
            .ToList();
        _statisticalReadRepositoryMock.Setup(
            r => r.GetDailyRevenueAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(daily);
        var handler = new GetSalesSummaryForChatQueryHandler(_statisticalReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new GetSalesSummaryForChatQuery { Limit = 10 }, CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(15);
        result.Value.Truncated.Should().BeTrue();
    }

    [Fact(DisplayName = "CHATTOOLS_108 - Unit - GetSalesSummaryForChatQueryHandler clamp limit vượt tối đa")]
    public async Task GetSalesSummary_ClampsLimitAboveMax()
    {
        _statisticalReadRepositoryMock.Setup(
            r => r.GetDailyRevenueAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = new GetSalesSummaryForChatQueryHandler(_statisticalReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new GetSalesSummaryForChatQuery { Limit = 999 }, CancellationToken.None)
            .ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "CHATTOOLS_110 - Unit - GetSalesSummaryForChatQueryHandler dùng đúng FromDate/ToDate được truyền vào")]
    public async Task GetSalesSummary_UsesExactFromDateToDate()
    {
        var fromDate = new DateOnly(2026, 7, 1);
        var toDate = new DateOnly(2026, 7, 28);
        DateTimeOffset capturedStart = default, capturedEnd = default;
        _statisticalReadRepositoryMock.Setup(
            r => r.GetDailyRevenueAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Callback<DateTimeOffset, DateTimeOffset, CancellationToken>((s, e, _) => { capturedStart = s; capturedEnd = e; })
            .ReturnsAsync([]);
        var handler = new GetSalesSummaryForChatQueryHandler(_statisticalReadRepositoryMock.Object, _dateProvider);
        await handler.Handle(new GetSalesSummaryForChatQuery { FromDate = fromDate, ToDate = toDate }, CancellationToken.None)
            .ConfigureAwait(true);
        var (expectedStart, _) = _dateProvider.VietnamDayRangeUtc(fromDate);
        var (_, expectedEndExclusive) = _dateProvider.VietnamDayRangeUtc(toDate);
        capturedStart.UtcDateTime.Should().Be(expectedStart);
        capturedEnd.UtcDateTime.Should().Be(expectedEndExclusive.AddTicks(-1));
    }

    [Fact(DisplayName = "CHATTOOLS_109 - Unit - GetTopSellingForChatQueryHandler trả danh sách bán chạy")]
    public async Task GetTopSelling_ValidRequest_ReturnsTopProducts()
    {
        var topProducts = new List<TopProductRevenueResponse> { new() { ProductName = "Vision", UnitsSold = 20, Revenue = 600000000 } };
        _statisticalReadRepositoryMock.Setup(
            r => r.GetTopProductsByRevenueAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topProducts);
        var handler = new GetTopSellingForChatQueryHandler(_statisticalReadRepositoryMock.Object, _dateProvider);
        var result = await handler.Handle(new GetTopSellingForChatQuery(), CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
        result.Value.Items[0].UnitsSold.Should().Be(20);
        result.Value.Items[0].Revenue.Should().Be(600000000);
    }

#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
