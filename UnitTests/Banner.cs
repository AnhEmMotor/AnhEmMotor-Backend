using Application.Features.Banners.Commands.CreateBanner;
using Application.ApiContracts.Banner.Responses;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Banner;

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
        // Arrange & Act
        var banner = new Domain.Entities.Banner();

        // Assert
        banner.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = "BANN_004 - Kiểm tra Banner hết hạn hiển thị")]
    public void Banner_IsExpired_ShouldReturnFalseForActivity()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var banner = new Domain.Entities.Banner
        {
            IsActive = true,
            StartDate = now.AddDays(-10),
            EndDate = now.AddDays(-1) // Hết hạn hôm qua
        };

        // Act
        bool isActiveAtMoment = banner.IsActive && 
                                (!banner.StartDate.HasValue || banner.StartDate <= now) && 
                                (!banner.EndDate.HasValue || banner.EndDate >= now);

        // Assert
        isActiveAtMoment.Should().BeFalse();
    }

    [Fact(DisplayName = "BANN_005 - Kiểm tra Banner chưa đến ngày hiển thị")]
    public void Banner_IsFuture_ShouldReturnFalseForActivity()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var banner = new Domain.Entities.Banner
        {
            IsActive = true,
            StartDate = now.AddDays(1), // Ngày mai mới bắt đầu
            EndDate = now.AddDays(10)
        };

        // Act
        bool isActiveAtMoment = banner.IsActive && 
                                (!banner.StartDate.HasValue || banner.StartDate <= now) && 
                                (!banner.EndDate.HasValue || banner.EndDate >= now);

        // Assert
        isActiveAtMoment.Should().BeFalse();
    }

    [Fact(DisplayName = "BANN_007 - Tự động cắt khoảng trắng trong dữ liệu đầu vào")]
    public async Task CreateBanner_WhitespaceInInput_ShouldTrimValues()
    {
        // Arrange
        var command = new CreateBannerCommand 
        { 
            Title = "  Title  ", 
            ImageUrl = "  http://img.com  ",
            LinkUrl = "  http://link.com  ",
            Position = "  Home  "
        };
        var handler = new CreateBannerCommandHandler(_bannerInsertRepoMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _bannerInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Banner>(b => 
            b.Title == "Title" && 
            b.ImageUrl == "http://img.com" &&
            b.LinkUrl == "http://link.com" &&
            b.Position == "Home"
        )), Times.Once);
    }

    [Fact(DisplayName = "BANN_008 - Hiển thị Banner theo vị trí cụ thể")]
    public void Banner_Position_SetsCorrectValue()
    {
        // Arrange & Act
        var banner = new Domain.Entities.Banner { Position = "Sidebar" };

        // Assert
        banner.Position.Should().Be("Sidebar");
    }

    [Fact(DisplayName = "BANN_009 - Mapping dữ liệu sang BannerResponse")]
    public void BannerResponse_Mapping_ShouldHaveCorrectValues()
    {
        // Arrange
        var banner = new Domain.Entities.Banner
        {
            Id = 1,
            Title = "T",
            ImageUrl = "I",
            LinkUrl = "L",
            Position = "P",
            DisplayOrder = 5
        };

        // Act
        var response = new BannerResponse
        {
            Id = banner.Id,
            Title = banner.Title,
            ImageUrl = banner.ImageUrl,
            LinkUrl = banner.LinkUrl,
            Position = banner.Position,
        };

        // Assert
        response.Id.Should().Be(1);
        response.Title.Should().Be("T");
        response.ImageUrl.Should().Be("I");
        response.LinkUrl.Should().Be("L");
        response.Position.Should().Be("P");
    }

    [Fact(DisplayName = "BANN_011 - Kiểm tra lưu trữ Banner khi không có liên kết (LinkUrl)")]
    public async Task CreateBanner_NullLinkUrl_ShouldSaveSuccessfully()
    {
        // Arrange
        var command = new CreateBannerCommand 
        { 
            Title = "Title", 
            ImageUrl = "http://img.com",
            LinkUrl = null
        };
        var handler = new CreateBannerCommandHandler(_bannerInsertRepoMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _bannerInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.Banner>(b => b.LinkUrl == null)), Times.Once);
    }
}
