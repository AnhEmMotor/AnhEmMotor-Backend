using Application.ApiContracts.Banner.Responses;
using Application.Features.Banners.Commands.CreateBanner;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;
using FluentAssertions;
using Moq;
using System;

namespace UnitTests;

public class Banner
{
    private readonly Mock<IBannerInsertRepository> _bannerInsertRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public Banner()
    {
        _bannerInsertRepoMock = new Mock<IBannerInsertRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "BANN_002 - Trạng thái hoạt động mặc định của Banner")]
    public void BannerEntity_DefaultIsActive_ShouldBeTrue()
    {
        var banner = new Domain.Entities.Banner();
        banner.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "BANN_004 - Kiểm tra Banner hết hạn hiển thị")]
    public void Banner_IsExpired_ShouldReturnFalseForActivity()
    {
        var now = DateTimeOffset.UtcNow;
        var banner = new Domain.Entities.Banner
        {
            IsActive = true,
            StartDate = now.AddDays(-10),
            EndDate = now.AddDays(-1)
        };
        bool isActiveAtMoment = banner.IsActive &&
            (!banner.StartDate.HasValue || banner.StartDate <= now) &&
            (!banner.EndDate.HasValue || banner.EndDate >= now);
        isActiveAtMoment.Should().BeFalse();
    }

    [Fact(DisplayName = "BANN_005 - Kiểm tra Banner chưa đến ngày hiển thị")]
    public void Banner_IsFuture_ShouldReturnFalseForActivity()
    {
        var now = DateTimeOffset.UtcNow;
        var banner = new Domain.Entities.Banner
        {
            IsActive = true,
            StartDate = now.AddDays(1),
            EndDate = now.AddDays(10)
        };
        bool isActiveAtMoment = banner.IsActive &&
            (!banner.StartDate.HasValue || banner.StartDate <= now) &&
            (!banner.EndDate.HasValue || banner.EndDate >= now);
        isActiveAtMoment.Should().BeFalse();
    }

    [Fact(DisplayName = "BANN_007 - Tự động cắt khoảng trắng trong dữ liệu đầu vào")]
    public async Task CreateBanner_WhitespaceInInput_ShouldTrimValues()
    {
        var command = new CreateBannerCommand
        {
            Title = "  Title  ",
            ImageUrl = "  http://img.com  ",
            LinkUrl = "  http://link.com  ",
            Position = "  Home  "
        };
        var handler = new CreateBannerCommandHandler(_bannerInsertRepoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _bannerInsertRepoMock.Verify(
            x => x.Add(
                It.Is<Domain.Entities.Banner>(
                    b => string.Compare(b.Title, "Title") == 0 &&
                        string.Compare(b.ImageUrl, "http://img.com") == 0 &&
                        string.Compare(b.LinkUrl, "http://link.com") == 0 &&
                        string.Compare(b.Position, "Home") == 0)),
            Times.Once);
    }

    [Fact(DisplayName = "BANN_008 - Hiển thị Banner theo vị trí cụ thể")]
    public void Banner_Position_SetsCorrectValue()
    {
        var banner = new Domain.Entities.Banner { Position = "Sidebar" };
        banner.Position.Should().Be("Sidebar");
    }

    [Fact(DisplayName = "BANN_009 - Mapping dữ liệu sang BannerResponse")]
    public void BannerResponse_Mapping_ShouldHaveCorrectValues()
    {
        var banner = new Domain.Entities.Banner
        {
            Id = 1,
            Title = "T",
            ImageUrl = "I",
            LinkUrl = "L",
            Position = "P",
            DisplayOrder = 5
        };
        var response = new BannerResponse
        {
            Id = banner.Id,
            Title = banner.Title,
            ImageUrl = banner.ImageUrl,
            LinkUrl = banner.LinkUrl,
            Position = banner.Position,
        };
        response.Id.Should().Be(1);
        response.Title.Should().Be("T");
        response.ImageUrl.Should().Be("I");
        response.LinkUrl.Should().Be("L");
        response.Position.Should().Be("P");
    }

    [Fact(DisplayName = "BANN_011 - Kiểm tra lưu trữ Banner khi không có liên kết (LinkUrl)")]
    public async Task CreateBanner_NullLinkUrl_ShouldSaveSuccessfully()
    {
        var command = new CreateBannerCommand { Title = "Title", ImageUrl = "http://img.com", LinkUrl = null };
        var handler = new CreateBannerCommandHandler(_bannerInsertRepoMock.Object, _unitOfWorkMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _bannerInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Banner>(b => b.LinkUrl == null)), Times.Once);
    }
}
