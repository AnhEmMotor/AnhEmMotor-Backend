using Application.Features.ChatTools.Common;
using Application.Features.ChatTools.Queries.GetProductDetailForChat;
using Application.Features.ChatTools.Queries.GetProductPriceListForChat;
using Application.Features.ChatTools.Queries.GetProductStockForChat;
using Application.Features.ChatTools.Queries.ListBrandsForChat;
using Application.Features.ChatTools.Queries.SearchProductsForChat;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using WebAPI.Controllers;

namespace ControllerTests;

public class PublicChatTools
{
    private readonly Mock<ISender> _senderMock;
    private readonly PublicChatToolsController _controller;
    private static readonly ChatToolEnvelopeMeta TestMeta = new(
        DateTimeOffset.UtcNow,
        "test-source",
        new Dictionary<string, string>(),
        null,
        null);

    public PublicChatTools()
    {
        _senderMock = new Mock<ISender>();
        _controller = new PublicChatToolsController(_senderMock.Object);
    }

    [Fact(DisplayName = "PUBLICCHATTOOLS_001 - Chỉ đúng 5 tool được phép, không thừa action nào")]
    public void Controller_ExposesExactlyFiveActions()
    {
        var actionNames = typeof(PublicChatToolsController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .ToList();
        actionNames.Should()
            .BeEquivalentTo(
                ["SearchProducts", "GetProductDetail", "GetProductStock", "GetProductPriceList", "ListBrands"]);
    }

    [Fact(DisplayName = "PUBLICCHATTOOLS_002 - Tìm sản phẩm - Happy Path trả đúng field")]
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
        var result = await _controller
            .SearchProducts(new SearchProductsForChatRequest { Keyword = "Wave" }, CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatProductSearchDto>>().Subject;
        actual.Items[0].ProductName.Should().Be("Wave Alpha");
    }

    [Fact(DisplayName = "PUBLICCHATTOOLS_003 - Chi tiết sản phẩm - Happy Path trả đúng field")]
    public async Task GetProductDetail_ValidRequest_ReturnsDetail()
    {
        var expected = ChatToolEnvelope<ChatProductDetailDto>.Wrap(
            new ChatToolResult<ChatProductDetailDto>(
                [new ChatProductDetailDto { ProductId = 5, ProductName = "SH 2024" }],
                1,
                false),
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<GetProductDetailForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller
            .GetProductDetail(new GetProductDetailForChatRequest { ProductId = 5 }, CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatProductDetailDto>>().Subject;
        actual.Items[0].ProductName.Should().Be("SH 2024");
    }

    [Fact(DisplayName = "PUBLICCHATTOOLS_004 - Tồn kho sản phẩm - Không trả số lượng chính xác")]
    public async Task GetProductStock_ValidRequest_HidesExactQuantity()
    {
        var expected = ChatToolEnvelope<ChatProductStockDto>.Wrap(
            new ChatToolResult<ChatProductStockDto>(
                [new ChatProductStockDto
                {
                    VariantId = 10,
                    VariantName = "Đỏ đen",
                    UnitPrice = 2500000,
                    StockQuantity = 50
                }, new ChatProductStockDto
                {
                    VariantId = 11,
                    VariantName = "Trắng",
                    UnitPrice = 2500000,
                    StockQuantity = 2
                }, new ChatProductStockDto
                {
                    VariantId = 12,
                    VariantName = "Xanh",
                    UnitPrice = 2500000,
                    StockQuantity = 0
                }],
                3,
                false),
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<GetProductStockForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller
            .GetProductStock(new GetProductStockForChatRequest { ProductId = 5 }, CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatProductStockPublicDto>>().Subject;
        actual.Items.Should().HaveCount(3);
        actual.Items[0].StockStatus.Should().Be("con_hang");
        actual.Items[1].StockStatus.Should().Be("sap_het");
        actual.Items[2].StockStatus.Should().Be("het_hang");
        okResult.Value.Should().NotBeOfType<ChatToolEnvelope<ChatProductStockDto>>();
    }

    [Fact(DisplayName = "PUBLICCHATTOOLS_005 - Danh sách giá sản phẩm - Happy Path trả đúng field")]
    public async Task GetProductPriceList_ValidRequest_ReturnsPrices()
    {
        var expected = ChatToolEnvelope<ChatProductPriceListItemDto>.Wrap(
            new ChatToolResult<ChatProductPriceListItemDto>(
                [new ChatProductPriceListItemDto { ProductName = "Wave Alpha", SellPrice = 21000000 }],
                1,
                false),
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<GetProductPriceListForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller
            .GetProductPriceList(new GetProductPriceListForChatRequest { Keyword = "Wave" }, CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatProductPriceListItemDto>>().Subject;
        actual.Items[0].SellPrice.Should().Be(21000000);
    }

    [Fact(DisplayName = "PUBLICCHATTOOLS_006 - Danh sách thương hiệu - Happy Path trả đúng field")]
    public async Task ListBrands_ValidRequest_ReturnsBrands()
    {
        var expected = ChatToolEnvelope<ChatBrandListItemDto>.Wrap(
            new ChatToolResult<ChatBrandListItemDto>(
                [new ChatBrandListItemDto { BrandName = "Honda", Origin = "Việt Nam" }],
                1,
                false),
            TestMeta);
        _senderMock.Setup(s => s.Send(It.IsAny<ListBrandsForChatQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var result = await _controller.ListBrands(new ListBrandsForChatRequest(), CancellationToken.None)
            .ConfigureAwait(true);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var actual = okResult.Value.Should().BeAssignableTo<ChatToolEnvelope<ChatBrandListItemDto>>().Subject;
        actual.Items[0].BrandName.Should().Be("Honda");
    }
}
