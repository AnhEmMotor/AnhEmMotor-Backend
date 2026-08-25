using Application.Features.Marketing.Queries.GetProductViewHistory;
using Application.Interfaces.Repositories.Product;
using Domain.Entities;
using FluentAssertions;
using Moq;
using ProductViewEntity = Domain.Entities.ProductView;
using ProductEntity = Domain.Entities.Product;

namespace UnitTests;

public class GetProductViewHistoryHandlerTests
{
    private readonly Mock<IProductViewRepository> _repositoryMock = new();
    private readonly GetProductViewHistoryQueryHandler _handler;

    public GetProductViewHistoryHandlerTests()
    {
        _handler = new GetProductViewHistoryQueryHandler(_repositoryMock.Object);
    }

    [Fact(DisplayName = "PVH_01 - Handler truyền đúng From/To và phân trang xuống repository")]
    public async Task Handle_PassesFromToAndPagingToRepository()
    {
        var from = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.FromHours(7));
        var to = new DateTimeOffset(2026, 8, 25, 23, 59, 59, TimeSpan.FromHours(7));
        _repositoryMock
            .Setup(r => r.GetProductViewHistoryPagedAsync("vision", 2, 20, from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ProductViewEntity>(), 0));

        var query = new GetProductViewHistoryQuery
        {
            PageNumber = 2,
            PageSize = 20,
            SearchKeyword = "vision",
            From = from,
            To = to
        };
        await _handler.Handle(query, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.GetProductViewHistoryPagedAsync("vision", 2, 20, from, to, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "PVH_02 - Handler không truyền From/To thì truyền null xuống repository")]
    public async Task Handle_NoRange_PassesNullsToRepository()
    {
        _repositoryMock
            .Setup(r => r.GetProductViewHistoryPagedAsync(null, 1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ProductViewEntity>(), 0));

        await _handler.Handle(new GetProductViewHistoryQuery(), CancellationToken.None);

        _repositoryMock.Verify(
            r => r.GetProductViewHistoryPagedAsync(null, 1, 10, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "PVH_03 - Handler map đúng trường entity sang response, ưu tiên ảnh VariantColor")]
    public async Task Handle_MapsEntityFieldsToResponse()
    {
        var entity = new ProductViewEntity
        {
            Id = Guid.NewGuid(),
            ProductId = 9,
            Product = new ProductEntity { Name = "Honda Vision", ProductVariants = [] },
            VariantId = 3,
            Variant = new ProductVariant { VariantName = "Phiên bản tiêu chuẩn", CoverImageUrl = null },
            VariantColorId = 5,
            VariantColor = new ProductVariantColor { ColorName = "Đỏ", CoverImageUrl = "https://img/red.png" },
            CustomerUserId = Guid.NewGuid(),
            CustomerUser = new ApplicationUser { FullName = "Nguyễn Văn A" },
            VisitorKey = "guest-abc",
            DwellTimeMs = 5200,
            ViewedAt = new DateTime(2026, 8, 24, 10, 30, 0, DateTimeKind.Utc)
        };
        _repositoryMock
            .Setup(r => r.GetProductViewHistoryPagedAsync(null, 1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ProductViewEntity> { entity }, 1));

        var result = await _handler.Handle(new GetProductViewHistoryQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.ProductName.Should().Be("Honda Vision");
        item.CustomerName.Should().Be("Nguyễn Văn A");
        item.VariantName.Should().Be("Phiên bản tiêu chuẩn");
        item.VariantColorName.Should().Be("Đỏ");
        item.ProductImageUrl.Should().Be("https://img/red.png");
        item.DwellTimeMs.Should().Be(5200);
        item.VisitorKey.Should().Be("guest-abc");
    }
}
