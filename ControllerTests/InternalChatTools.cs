using Application.Features.ChatTools.Common;
using Application.Features.ChatTools.Queries.GetLowStockProductsForChat;
using Application.Features.ChatTools.Queries.GetOrderStatusForChat;
using Application.Features.ChatTools.Queries.GetProductStockForChat;
using Application.Features.ChatTools.Queries.GetSalesSummaryForChat;
using Application.Features.ChatTools.Queries.GetTopSellingForChat;
using Application.Features.ChatTools.Queries.SearchProductsForChat;
using Application.Interfaces.Services;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;

namespace ControllerTests;

public class InternalChatTools
{
    private readonly Mock<ISender> _senderMock;
    private readonly InternalChatToolsController _controller;
    private static readonly ChatToolEnvelopeMeta TestMeta = new(
        DateTimeOffset.UtcNow,
        "test-source",
        new Dictionary<string, string>(),
        null,
        null);

    public InternalChatTools()
    {
        _senderMock = new Mock<ISender>();
        var catalogProviderMock = new Mock<IChatToolCatalogProvider>();
        _controller = new InternalChatToolsController(_senderMock.Object, catalogProviderMock.Object);
    }

    #pragma warning disable IDE0079
    #pragma warning disable CRR0035

    [Fact(DisplayName = "CHATTOOLS_001 - Tìm sản phẩm - Happy Path trả đúng field")]
    public async Task SearchProducts_ValidRequest_ReturnsProducts()
    {
        var expected = ChatToolEnvelope<ChatProductSearchDto>.Wrap(
            new ChatToolResult<ChatProductSearchDto>(
                [new ChatProductSearchDto { ProductId = 1, ProductName = "Wave Alpha", BrandName = "Honda" }],
                1,
                false),
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<SearchProductsForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller.SearchProducts(
            new SearchProductsForChatRequest { Keyword = "Wave" },
            CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatProductSearchDto>>().Subject;
        actual.TotalCount.Should().Be(1);
        actual.Items[0].ProductName.Should().Be("Wave Alpha");
    }

    [Fact(DisplayName = "CHATTOOLS_002 - Tìm sản phẩm - Không có quyền không trả số liệu")]
    public async Task SearchProducts_NoPermission_ThrowsAndReturnsNoData()
    {
        _senderMock.Setup(s => s.Send(It.IsAny<SearchProductsForChatQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.SearchProducts(
                new SearchProductsForChatRequest { Keyword = "Wave" },
                CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "CHATTOOLS_003 - Tồn kho sản phẩm - Happy Path trả đúng field")]
    public async Task GetProductStock_ValidRequest_ReturnsStock()
    {
        var expected = ChatToolEnvelope<ChatProductStockDto>.Wrap(
            new ChatToolResult<ChatProductStockDto>(
                [new ChatProductStockDto { VariantId = 10, UnitPrice = 2500000, StockQuantity = 50 }],
                1,
                false),
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<GetProductStockForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller.GetProductStock(
            new GetProductStockForChatRequest { ProductId = 5 },
            CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatProductStockDto>>().Subject;
        actual.Items[0].StockQuantity.Should().Be(50);
    }

    [Fact(DisplayName = "CHATTOOLS_004 - Tồn kho sản phẩm - Không có quyền không trả số liệu")]
    public async Task GetProductStock_NoPermission_ThrowsAndReturnsNoData()
    {
        _senderMock.Setup(s => s.Send(It.IsAny<GetProductStockForChatQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetProductStock(
                new GetProductStockForChatRequest { ProductId = 5 },
                CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "CHATTOOLS_005 - Sản phẩm sắp hết hàng - Happy Path trả đúng field")]
    public async Task GetLowStockProducts_ValidRequest_ReturnsLowStock()
    {
        var expected = ChatToolEnvelope<ChatLowStockProductDto>.Wrap(
            new ChatToolResult<ChatLowStockProductDto>(
                [new ChatLowStockProductDto { ProductName = "Vision - Đỏ", StockQuantity = 2, Status = "Sắp hết" }],
                1,
                false),
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<GetLowStockProductsForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller.GetLowStockProducts(
            new GetLowStockProductsForChatRequest(),
            CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatLowStockProductDto>>().Subject;
        actual.Items[0].Status.Should().Be("Sắp hết");
    }

    [Fact(DisplayName = "CHATTOOLS_006 - Sản phẩm sắp hết hàng - Không có quyền không trả số liệu")]
    public async Task GetLowStockProducts_NoPermission_ThrowsAndReturnsNoData()
    {
        _senderMock.Setup(s => s.Send(It.IsAny<GetLowStockProductsForChatQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetLowStockProducts(new GetLowStockProductsForChatRequest(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "CHATTOOLS_007 - Trạng thái đơn hàng - Happy Path trả đúng field")]
    public async Task GetOrderStatus_ValidRequest_ReturnsOrderStatus()
    {
        var expected = ChatToolEnvelope<ChatOrderStatusDto>.WrapSingle(
            new ChatOrderStatusDto { OrderId = 123, StatusId = "completed", Total = 5000000 },
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<GetOrderStatusForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller.GetOrderStatus(
            new GetOrderStatusForChatRequest { Keyword = "Nguyễn Văn A" },
            CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatOrderStatusDto>>().Subject;
        actual.Items[0].OrderId.Should().Be(123);
        actual.Items[0].StatusId.Should().Be("completed");
    }

    [Fact(DisplayName = "CHATTOOLS_008 - Trạng thái đơn hàng - Không có quyền không trả số liệu")]
    public async Task GetOrderStatus_NoPermission_ThrowsAndReturnsNoData()
    {
        _senderMock.Setup(s => s.Send(It.IsAny<GetOrderStatusForChatQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetOrderStatus(
                new GetOrderStatusForChatRequest { Keyword = "Nguyễn Văn A" },
                CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "CHATTOOLS_009 - Tóm tắt doanh thu - Happy Path trả đúng field")]
    public async Task GetSalesSummary_ValidRequest_ReturnsRevenue()
    {
        var expected = ChatToolEnvelope<ChatDailyRevenueDto>.Wrap(
            new ChatToolResult<ChatDailyRevenueDto>(
                [new ChatDailyRevenueDto { ReportDay = DateOnly.FromDateTime(DateTime.Now), TotalRevenue = 3000000 }],
                1,
                false),
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<GetSalesSummaryForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller.GetSalesSummary(new GetSalesSummaryForChatRequest(), CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatDailyRevenueDto>>().Subject;
        actual.Items[0].TotalRevenue.Should().Be(3000000);
    }

    [Fact(DisplayName = "CHATTOOLS_010 - Tóm tắt doanh thu - Không có quyền không trả số liệu")]
    public async Task GetSalesSummary_NoPermission_ThrowsAndReturnsNoData()
    {
        _senderMock.Setup(s => s.Send(It.IsAny<GetSalesSummaryForChatQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetSalesSummary(new GetSalesSummaryForChatRequest(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "CHATTOOLS_011 - Sản phẩm bán chạy - Happy Path trả đúng field")]
    public async Task GetTopSelling_ValidRequest_ReturnsTopProducts()
    {
        var expected = ChatToolEnvelope<ChatTopSellingProductDto>.Wrap(
            new ChatToolResult<ChatTopSellingProductDto>(
                [new ChatTopSellingProductDto { ProductName = "Vision", UnitsSold = 20, Revenue = 600000000 }],
                1,
                false),
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<GetTopSellingForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller.GetTopSelling(new GetTopSellingForChatRequest(), CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatTopSellingProductDto>>().Subject;
        actual.Items[0].UnitsSold.Should().Be(20);
    }

    [Fact(DisplayName = "CHATTOOLS_012 - Sản phẩm bán chạy - Không có quyền không trả số liệu")]
    public async Task GetTopSelling_NoPermission_ThrowsAndReturnsNoData()
    {
        _senderMock.Setup(s => s.Send(It.IsAny<GetTopSellingForChatQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetTopSelling(new GetTopSellingForChatRequest(), CancellationToken.None))
            .ConfigureAwait(true);
    }

#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
