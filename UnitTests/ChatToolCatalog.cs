using Application.Common.Models;
using Application.Features.ManagerChat.Queries.GetChatToolCatalog;
using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests;

public class ChatToolCatalog
{
    private static readonly string[] ExpectedNames =
    [
        "search_products", "get_product_stock", "get_low_stock_products",
        "get_order_status", "get_sales_summary", "get_top_selling",
    ];

    [Fact(DisplayName = "CATALOG_01 - Unit - GetChatToolCatalogQueryHandler map đúng Name/Label từ provider")]
    public async Task Handle_MapsProviderEntriesToDtos()
    {
        var providerMock = new Mock<IChatToolCatalogProvider>();
        providerMock.Setup(p => p.GetCatalog()).Returns(
        [
            new ChatToolCatalogEntry("search_products", "products/search", "Tìm sản phẩm"),
        ]);
        var handler = new GetChatToolCatalogQueryHandler(providerMock.Object);

        var result = await handler.Handle(new GetChatToolCatalogQuery(), CancellationToken.None)
            .ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value![0].Name.Should().Be("search_products");
        result.Value[0].Label.Should().Be("Tìm sản phẩm");
    }

    [Fact(DisplayName = "CATALOG_02 - Guard - chat-tools-catalog.json khớp đúng 6 tool đã implement, không lệch tên/route")]
    public void ChatToolCatalogProvider_DocDungFileThat_KhongLech6Tool()
    {
        var provider = new ChatToolCatalogProvider(NullLogger<ChatToolCatalogProvider>.Instance);

        var catalog = provider.GetCatalog();

        catalog.Should().HaveCount(6, "chat-tools-catalog.json phải khớp đúng 6 tool Stage 3 đã implement ở InternalChatToolsController");
        catalog.Select(e => e.Name).Should().BeEquivalentTo(ExpectedNames,
            "thêm/xoá tool phải sửa chat-tools-catalog.json — đây là nguồn duy nhất cho cả .NET và sidecar Python");
        catalog.Should().OnlyContain(e => !string.IsNullOrWhiteSpace(e.Label), "mỗi tool phải có label tiếng Việt cho FE");
        catalog.Should().OnlyContain(e => !string.IsNullOrWhiteSpace(e.Path), "mỗi tool phải có path khớp route InternalChatToolsController");
    }
}
