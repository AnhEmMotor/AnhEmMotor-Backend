using Application.ApiContracts.News.Responses;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
        await _factory.ResetDatabaseAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "NEWS_006 - Lấy danh sách tin tức chỉ bao gồm các bài đã xuất bản")]
    public async Task GetNewsList_OnlyReturnsPublishedNews()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.News
                .AddRange(
                    new Domain.Entities.News { Title = "P1", Slug = "p1", IsPublished = true, Content = "C" },
                    new Domain.Entities.News { Title = "P2", Slug = "p2", IsPublished = true, Content = "C" },
                    new Domain.Entities.News { Title = "P3", Slug = "p3", IsPublished = true, Content = "C" },
                    new Domain.Entities.News { Title = "D1", Slug = "d1", IsPublished = false, Content = "C" },
                    new Domain.Entities.News { Title = "D2", Slug = "d2", IsPublished = false, Content = "C" });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.GetAsync("/api/v1/news", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response!.Content
            .ReadFromJsonAsync<PagedResult<NewsResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        pagedResult.Should().NotBeNull();
        var content = pagedResult!.Items;
        content!.Should().NotBeNull();
        content!.All(n => n.IsPublished).Should().BeTrue();
        content!.Should().HaveCount(3);
    }

    [Fact(DisplayName = "NEWS_007 - Truy vấn chi tiết tin tức bằng đường dẫn Slug")]
    public async Task GetNewsBySlug_ValidSlug_ReturnsCorrectNews()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.News
                .Add(
                    new Domain.Entities.News
                    {
                        Title = "Target",
                        Slug = "tin-abc",
                        IsPublished = true,
                        Content = "Full Content"
                    });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.GetAsync("/api/v1/news/tin-abc", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<NewsResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Title.Should().Be("Target");
        content.Slug.Should().Be("tin-abc");
    }

    [Fact(DisplayName = "NEWS_011 - Kiểm tra tính đúng đắn của phân trang tin tức")]
    public async Task GetNewsList_WithPaging_ReturnsCorrectPage()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            for (int i = 1; i <= 10; i++)
            {
                db.News
                    .Add(
                        new Domain.Entities.News
                        {
                            Title = $"News {i}",
                            Slug = $"news-{i}",
                            IsPublished = true,
                            Content = "C"
                        });
            }
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            "/api/v1/news?Page=1&PageSize=2&Sorts=id",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (response!.StatusCode != HttpStatusCode.OK)
        {
            var error = await response!.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            throw new Exception($"NEWS_011 failed with {response!.StatusCode}: {error}");
        }
        var pagedResult = await response!.Content
            .ReadFromJsonAsync<PagedResult<NewsResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        pagedResult.Should().NotBeNull();
        pagedResult.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "NEWS_012 - Đảm bảo tính toàn vẹn của thông tin Metadata SEO")]
    public async Task CreateNews_WithSEO_SavesMetadataCorrectly()
    {
        var payload = new
        {
            title = "SEO News",
            content = "C",
            meta_title = "Meta T",
            metaTitle = "Meta T",
            meta_description = "Meta D",
            metaDescription = "Meta D",
            meta_keywords = "K1, K2",
            metaKeywords = "K1, K2",
            is_published = true,
            isPublished = true
        };
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            ["Domain.Constants.Permission.Permissions.News.Create"],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.PostAsJsonAsync("/api/v1/news", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var news = await db.News
            .FirstOrDefaultAsync(n => string.Compare(n.Title, "SEO News") == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        news.Should().NotBeNull();
        news!.MetaTitle.Should().Be("Meta T");
        news.MetaDescription.Should().Be("Meta D");
        news.MetaKeywords.Should().Be("K1, K2");
    }

    [Fact(DisplayName = "NEWS_013 - Tạo bài viết với đầy đủ thông tin SEO và Danh mục")]
    public async Task CreateNews_WithFullSEOAndCategory_ReturnsCreated()
    {
        int categoryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var cat = new NewsCategory { Name = "Tech", Slug = "tech" };
            db.NewsCategories.Add(cat);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            categoryId = cat.Id;
        }
        var payload = new
        {
            title = "SEO News 2",
            content = "Full content",
            meta_title = "SEO Meta Title",
            meta_description = "SEO Meta Description",
            slug = "seo-news-slug",
            category_id = categoryId,
            is_published = true
        };
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            ["Domain.Constants.Permission.Permissions.News.Create"],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.PostAsJsonAsync("/api/v1/news", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var news = await db.News
                .FirstOrDefaultAsync(
                    n => string.Compare(n.Slug, "seo-news-slug") == 0,
                    TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            news.Should().NotBeNull();
            news!.CategoryId.Should().Be(categoryId);
            news.MetaTitle.Should().Be("SEO Meta Title");
        }
    }

    [Fact(DisplayName = "NEWS_015 - Thay đổi trạng thái hiển thị bài viết")]
    public async Task UpdateNewsStatus_ToPublished_SetsIsPublishedTrue()
    {
        int newsId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var news = new Domain.Entities.News { Title = "Draft", Slug = "draft", IsPublished = false, Content = "C" };
            db.News.Add(news);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            newsId = news.Id;
        }
        var payload = new { id = newsId, is_published = true };
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            ["Domain.Constants.Permission.Permissions.News.Update"],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/news/{newsId}/status",
            payload,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var news = await db.News.FindAsync([newsId], TestContext.Current.CancellationToken).ConfigureAwait(true);
            news!.IsPublished.Should().BeTrue();
            news.PublishedDate.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "NEWS_017 - Gán tác giả cho bài viết")]
    public async Task AssignAuthor_ValidAuthorId_UpdatesNews()
    {
        int newsId;
        Guid authorId;
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var news = new Domain.Entities.News { Title = "News", Slug = "news-auth", Content = "C" };
            db.News.Add(news);
            var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
                _factory.Services,
                $"author_{uniqueId}",
                "Password123!",
                ["Domain.Constants.Permission.Permissions.News.Create"],
                TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            authorId = user.Id;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            newsId = news.Id;
        }
        var payload = new { id = newsId, title = "Updated Title", author_id = authorId, content = "New content" };
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"admin_{uniqueId}",
            "Password123!",
            ["Domain.Constants.Permission.Permissions.News.Update"],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"admin_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.PutAsJsonAsync($"/api/v1/news/{newsId}", payload).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var news = await db.News.FindAsync([newsId], TestContext.Current.CancellationToken).ConfigureAwait(true);
            news!.AuthorId.Should().Be(authorId);
        }
    }

    [Fact(DisplayName = "NEWS_018 - Xóa bài viết (Soft Delete)")]
    public async Task DeleteNews_SoftDelete_SetsDeletedAt()
    {
        int newsId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var news = new Domain.Entities.News { Title = "To Delete", Slug = "to-delete", Content = "C" };
            db.News.Add(news);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            newsId = news.Id;
        }
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            ["Domain.Constants.Permission.Permissions.News.Delete"],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.DeleteAsync($"/api/v1/news/{newsId}", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var news = await db.All<Domain.Entities.News>()
                .FirstOrDefaultAsync(n => n.Id == newsId, TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            news!.DeletedAt.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "NEWS_019 - Lọc bài viết theo danh mục (Category)")]
    public async Task GetNewsList_FilterByCategory_ReturnsOnlyCategoryNews()
    {
        int cat1Id, cat2Id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var cat1 = new NewsCategory { Name = "Cat 1", Slug = "cat-1" };
            var cat2 = new NewsCategory { Name = "Cat 2", Slug = "cat-2" };
            db.NewsCategories.AddRange(cat1, cat2);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            cat1Id = cat1.Id;
            cat2Id = cat2.Id;
            db.News
                .AddRange(
                    new Domain.Entities.News
                    {
                        Title = "N1",
                        Slug = "n1",
                        CategoryId = cat1Id,
                        IsPublished = true,
                        Content = "C"
                    },
                    new Domain.Entities.News
                    {
                        Title = "N2",
                        Slug = "n2",
                        CategoryId = cat1Id,
                        IsPublished = true,
                        Content = "C"
                    },
                    new Domain.Entities.News
                    {
                        Title = "N3",
                        Slug = "n3",
                        CategoryId = cat2Id,
                        IsPublished = true,
                        Content = "C"
                    });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/news?Filters=CategoryId=={cat1Id}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response!.Content
            .ReadFromJsonAsync<PagedResult<NewsResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        pagedResult!.Items.Should().HaveCount(2);
        pagedResult.Items
            .All(n => string.Compare(n.Title, "N1") == 0 || string.Compare(n.Title, "N2") == 0)
            .Should()
            .BeTrue();
    }
}

