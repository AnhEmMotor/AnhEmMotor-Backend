using Application.Common.Helper;
using Application.Common.Models;
using Application.Features.News.Commands.CreateNews;
using Application.Features.News.Queries.GetNewsBySlug;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests;

public class News
{
    private readonly Mock<INewsReadRepository> _newsReadRepoMock;
    private readonly Mock<INewsInsertRepository> _newsInsertRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public News()
    {
        _newsReadRepoMock = new Mock<INewsReadRepository>();
        _newsInsertRepoMock = new Mock<INewsInsertRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact(DisplayName = "NEWS_001 - Tạo tin tức với Slug tự động từ tiêu đề")]
    public async Task CreateNews_NoSlugProvided_GeneratesSlugFromTitle()
    {
        // Arrange
        var command = new CreateNewsCommand { Title = "Tin tức mới", Content = "C" };
        _newsReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.News?)null);

        var handler = new CreateNewsCommandHandler(_newsInsertRepoMock.Object, _newsReadRepoMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _newsInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.News>(n => n.Slug == "tin-tuc-moi")), Times.Once);
    }

    [Fact(DisplayName = "NEWS_002 - Tạo tin tức với Slug tùy chỉnh từ người dùng")]
    public async Task CreateNews_CustomSlugProvided_UsesCustomSlug()
    {
        // Arrange
        var command = new CreateNewsCommand { Title = "Title", Slug = "slug-tuy-chinh", Content = "C" };
        _newsReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.News?)null);

        var handler = new CreateNewsCommandHandler(_newsInsertRepoMock.Object, _newsReadRepoMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _newsInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.News>(n => n.Slug == "slug-tuy-chinh")), Times.Once);
    }

    [Fact(DisplayName = "NEWS_003 - Chặn tạo tin tức khi Slug bị trùng lặp")]
    public async Task CreateNews_DuplicateSlug_ReturnsFailure()
    {
        // Arrange
        var command = new CreateNewsCommand { Title = "Title", Slug = "tin-da-co", Content = "C" };
        _newsReadRepoMock.Setup(x => x.GetBySlugAsync("tin-da-co", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.News { Slug = "tin-da-co" });

        var handler = new CreateNewsCommandHandler(_newsInsertRepoMock.Object, _newsReadRepoMock.Object, _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("đã tồn tại"));
    }

    [Fact(DisplayName = "NEWS_004 - Tự động gán ngày xuất bản khi kích hoạt tin tức")]
    public async Task CreateNews_PublishedTrue_SetsPublishedDate()
    {
        // Arrange
        var command = new CreateNewsCommand { Title = "Title", IsPublished = true, Content = "C" };
        _newsReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.News?)null);

        var handler = new CreateNewsCommandHandler(_newsInsertRepoMock.Object, _newsReadRepoMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _newsInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.News>(n => n.PublishedDate != null)), Times.Once);
    }

    [Fact(DisplayName = "NEWS_005 - Không gán ngày xuất bản cho bản nháp")]
    public async Task CreateNews_PublishedFalse_PublishedDateIsNull()
    {
        // Arrange
        var command = new CreateNewsCommand { Title = "Title", IsPublished = false, Content = "C" };
        _newsReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.News?)null);

        var handler = new CreateNewsCommandHandler(_newsInsertRepoMock.Object, _newsReadRepoMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _newsInsertRepoMock.Verify(x => x.Add(It.Is<Domain.Entities.News>(n => n.PublishedDate == null)), Times.Once);
    }

    [Fact(DisplayName = "NEWS_008 - Xử lý lỗi khi truy vấn Slug không tồn tại")]
    public async Task GetBySlug_NotExists_ReturnsNotFound()
    {
        // Arrange
        _newsReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.News?)null);

        var handler = new GetNewsBySlugQueryHandler(_newsReadRepoMock.Object);

        // Act
        var result = await handler.Handle(new GetNewsBySlugQuery { Slug = "non-existent" }, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "News.NotFound");
    }

    [Fact(DisplayName = "NEWS_009 - Chuẩn hóa tiếng Việt có dấu sang Slug không dấu")]
    public void SlugHelper_VietnameseTitle_GeneratesCorrectSlug()
    {
        // Act
        var slug = SlugHelper.GenerateSlug("Học C# tại Anh Em Motor");

        // Assert
        slug.Should().Be("hoc-c-tai-anh-em-motor");
    }

    [Fact(DisplayName = "NEWS_010 - Kiểm tra giới hạn độ dài của đường dẫn Slug")]
    public void SlugHelper_VeryLongTitle_TruncatesSlug()
    {
        // Arrange
        var longTitle = new string('a', 300);

        // Act
        var slug = SlugHelper.GenerateSlug(longTitle);

        // Assert
        slug.Length.Should().BeLessOrEqualTo(255);
    }
}
