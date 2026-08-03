using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Queries.GetPersonalizedRecommendations;
using Application.Features.Products.Queries.GetProductsList;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services;
using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Moq;

namespace UnitTests;

public class ProductRecommendation
{
    private readonly Mock<IProductViewRepository> _productViewRepoMock = new();
    private readonly Mock<ICurrentUserContext> _currentUserContextMock = new();
    private readonly Mock<ISender> _senderMock = new();

    private GetPersonalizedRecommendationsQueryHandler CreateHandler() =>
        new(_productViewRepoMock.Object, _currentUserContextMock.Object, _senderMock.Object);

    private void SetupSenderCapture(Action<GetProductsListQuery> onSend)
    {
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetProductsListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<PagedResult<ProductListStoreResponse>>>, CancellationToken>(
                (req, _) => onSend((GetProductsListQuery)req))
            .ReturnsAsync(Result<PagedResult<ProductListStoreResponse>>.Success(new PagedResult<ProductListStoreResponse>([], 0, 1, 4)));
    }

    [Fact(DisplayName = "RECO_01 - Có lịch sử xem lệch hẳn về 1 category -> ưu tiên đúng category đó")]
    public async Task Handler_WithSkewedViewHistory_PrioritizesDominantCategory()
    {
        var customerId = Guid.NewGuid();
        _currentUserContextMock.Setup(c => c.GetUserIdOrNull()).Returns(customerId);

        var now = DateTimeOffset.UtcNow;
        _productViewRepoMock
            .Setup(
                r => r.GetRecentViewsAsync(
                    customerId,
                    null,
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [
                    new ProductViewSample(5, 120_000, now.AddDays(-1)),
                    new ProductViewSample(5, 90_000, now.AddDays(-2)),
                    new ProductViewSample(8, 5_000, now.AddDays(-60))
                ]);

        GetProductsListQuery? capturedQuery = null;
        SetupSenderCapture(q => capturedQuery = q);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetPersonalizedRecommendationsQuery(4, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedQuery.Should().NotBeNull();
        capturedQuery!.CategoryIds.Should().NotBeEmpty();
        capturedQuery.CategoryIds[0].Should().Be(5, "category 5 có 2 lượt xem gần đây với dwell time cao hơn hẳn category 8 (1 lượt, xem lâu trước đó)");
    }

    [Fact(DisplayName = "RECO_02 - Không có định danh nào (chưa đăng nhập, không có visitorKey) -> fallback CategoryIds rỗng")]
    public async Task Handler_WithNoIdentity_FallsBackToEmptyCategoryFilter()
    {
        _currentUserContextMock.Setup(c => c.GetUserIdOrNull()).Returns((Guid?)null);

        GetProductsListQuery? capturedQuery = null;
        SetupSenderCapture(q => capturedQuery = q);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetPersonalizedRecommendationsQuery(4, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedQuery.Should().NotBeNull();
        capturedQuery!.CategoryIds.Should().BeEmpty();
        _productViewRepoMock.Verify(
            r => r.GetRecentViewsAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
