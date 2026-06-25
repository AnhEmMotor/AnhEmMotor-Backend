using Application.Features.Banners.Commands.CreateBanner;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;
using Application.Interfaces.Services;
using Moq;
using System;

namespace UnitTests;

public class Banner
{
    private readonly Mock<IBannerInsertRepository> _bannerInsertRepoMock;
    private readonly Mock<ICurrentUserContext> _tokenAccessorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public Banner()
    {
        _bannerInsertRepoMock = new Mock<IBannerInsertRepository>();
        _tokenAccessorMock = new Mock<ICurrentUserContext>();
        _tokenAccessorMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "BANN_007 - Tự động cắt khoảng trắng trong dữ liệu đầu vào")]
    public async Task CreateBanner_WhitespaceInInventoryReceipt_ShouldTrimValues()
    {
        var command = new CreateBannerCommand
        {
            Title = "  Title  ",
            DesktopImageUrl = "  http://img.com  ",
            CtaLink = "  http://link.com  "
        };
        var handler = new CreateBannerCommandHandler(
            _bannerInsertRepoMock.Object,
            _tokenAccessorMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _bannerInsertRepoMock.Verify(
            x => x.Add(
                It.Is<Domain.Entities.Banner>(
                    b => string.Compare(b.Title, "Title") == 0 &&
                        string.Compare(b.DesktopImageUrl, "http://img.com") == 0 &&
                        string.Compare(b.CtaLink, "http://link.com") == 0)),
            Times.Once);
    }

    [Fact(DisplayName = "BANN_011 - Kiểm tra lưu trữ Banner khi không có liên kết (LinkUrl)")]
    public async Task CreateBanner_NullLinkUrl_ShouldSaveSuccessfully()
    {
        var command = new CreateBannerCommand { Title = "Title", DesktopImageUrl = "http://img.com", CtaLink = null };
        var handler = new CreateBannerCommandHandler(
            _bannerInsertRepoMock.Object,
            _tokenAccessorMock.Object,
            _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _bannerInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Banner>(b => b.CtaLink == null)), Times.Once);
    }
}
