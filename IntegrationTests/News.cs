using Application.Features.News.Commands.CreateNews;
using Application.Features.News.Queries.GetNewsList;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

public class News : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public News(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "NEWS_006 - Lấy danh sách tin tức chỉ bao gồm các bài đã xuất bản")]
    public async Task GetNewsList_OnlyReturnsPublishedNews()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.News.AddRange(
                new Domain.Entities.News { Title = "P1", Slug = "p1", IsPublished = true, Content = "C" },
                new Domain.Entities.News { Title = "P2", Slug = "p2", IsPublished = true, Content = "C" },
                new Domain.Entities.News { Title = "P3", Slug = "p3", IsPublished = true, Content = "C" },
                new Domain.Entities.News { Title = "D1", Slug = "d1", IsPublished = false, Content = "C" },
                new Domain.Entities.News { Title = "D2", Slug = "d2", IsPublished = false, Content = "C" }
            );
            await db.SaveChangesAsync().ConfigureAwait(true);
        }

        // Act
        var response = await _client.GetAsync("/api/v1/news").ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<Application.ApiContracts.News.Responses.NewsResponse>>().ConfigureAwait(true);
        content.Should().NotBeNull();
        content.All(n => n.IsPublished).Should().BeTrue();
        content.Should().HaveCount(3);
    }

    [Fact(DisplayName = "NEWS_007 - Truy vấn chi tiết tin tức bằng đường dẫn Slug")]
    public async Task GetNewsBySlug_ValidSlug_ReturnsCorrectNews()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.News.Add(new Domain.Entities.News { Title = "Target", Slug = "tin-abc", IsPublished = true, Content = "Full Content" });
            await db.SaveChangesAsync().ConfigureAwait(true);
        }

        // Act
        var response = await _client.GetAsync("/api/v1/news/tin-abc").ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Application.ApiContracts.News.Responses.NewsResponse>().ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Title.Should().Be("Target");
        content.Slug.Should().Be("tin-abc");
    }

    [Fact(DisplayName = "NEWS_011 - Kiểm tra tính đúng đắn của phân trang tin tức")]
    public async Task GetNewsList_WithPaging_ReturnsCorrectPage()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for (int i = 1; i <= 10; i++)
            {
                db.News.Add(new Domain.Entities.News { Title = $"News {i}", Slug = $"news-{i}", IsPublished = true, Content = "C" });
            }
            await db.SaveChangesAsync().ConfigureAwait(true);
        }

        // Act
        var response = await _client.GetAsync("/api/v1/news?Page=1&PageSize=2").ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<Application.ApiContracts.News.Responses.NewsResponse>>().ConfigureAwait(true);
        content.Should().NotBeNull();
        content.Should().HaveCount(2);
    }

    [Fact(DisplayName = "NEWS_012 - Đảm bảo tính toàn vẹn của thông tin Metadata SEO")]
    public async Task CreateNews_WithSEO_SavesMetadataCorrectly()
    {
        // Arrange
        var command = new CreateNewsCommand 
        { 
            Title = "SEO News", 
            Content = "C",
            MetaTitle = "Meta T",
            MetaDescription = "Meta D",
            MetaKeywords = "K1, K2"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/news", command).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var news = await db.News.FirstOrDefaultAsync(n => n.Title == "SEO News").ConfigureAwait(true);
            news.Should().NotBeNull();
            news!.MetaTitle.Should().Be("Meta T");
            news.MetaDescription.Should().Be("Meta D");
            news.MetaKeywords.Should().Be("K1, K2");
        }
    }
}
