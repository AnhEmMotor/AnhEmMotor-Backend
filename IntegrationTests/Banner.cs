using Application.ApiContracts.Banner.Responses;
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

public class Banner : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public Banner(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
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

    [Fact(DisplayName = "BANN_003 - Lấy danh sách Banner đang hoạt động")]
    public async Task GetActiveBanners_OnlyReturnsActiveOnes()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Banners
                .AddRange(
                    new Domain.Entities.Banner { Title = "A1", ImageUrl = "I1", IsActive = true },
                    new Domain.Entities.Banner { Title = "A2", ImageUrl = "I2", IsActive = true },
                    new Domain.Entities.Banner { Title = "I1", ImageUrl = "I3", IsActive = false });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.GetAsync("/api/v1/banners/active", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<List<BannerResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content.Should().HaveCount(2);
    }

    [Fact(DisplayName = "BANN_006 - Sắp xếp Banner theo thứ tự hiển thị")]
    public async Task GetActiveBanners_ReturnsSortedByDisplayOrder()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Banners
                .AddRange(
                    new Domain.Entities.Banner { Title = "B1", ImageUrl = "I1", IsActive = true, DisplayOrder = 10 },
                    new Domain.Entities.Banner { Title = "B2", ImageUrl = "I2", IsActive = true, DisplayOrder = 1 },
                    new Domain.Entities.Banner { Title = "B3", ImageUrl = "I3", IsActive = true, DisplayOrder = 5 });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.GetAsync("/api/v1/banners/active", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<BannerResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content![0].DisplayOrder.Should().Be(1);
        content[1].DisplayOrder.Should().Be(5);
        content[2].DisplayOrder.Should().Be(10);
    }

    [Fact(DisplayName = "BANN_010 - Kiểm tra logic lọc thời hạn banner trong Repository")]
    public async Task GetActiveBanners_FiltersByDateRange()
    {
        var now = DateTimeOffset.UtcNow;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Banners
                .AddRange(
                    new Domain.Entities.Banner
                    {
                        Title = "Valid",
                        ImageUrl = "I1",
                        IsActive = true,
                        StartDate = now.AddDays(-1),
                        EndDate = now.AddDays(1)
                    },
                    new Domain.Entities.Banner
                    {
                        Title = "Expired",
                        ImageUrl = "I2",
                        IsActive = true,
                        EndDate = now.AddDays(-1)
                    },
                    new Domain.Entities.Banner
                    {
                        Title = "Future",
                        ImageUrl = "I3",
                        IsActive = true,
                        StartDate = now.AddDays(1)
                    });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var response = await _client.GetAsync("/api/v1/banners/active", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var content = await response.Content
            .ReadFromJsonAsync<List<BannerResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content.Should().HaveCount(1);
        content![0].Title.Should().Be("Valid");
    }

    [Fact(DisplayName = "BANN_012 - Kiểm tra tính toàn vẹn của dữ liệu ảnh (ImageUrl)")]
    public async Task CreateBanner_ValidImageUrl_SavesCorrectly()
    {
        var payload = new { title = "Integrity Test", image_url = "http://anh-em-motor.com/banner.jpg" };
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            ["Permissions.Banners.Create"],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.PostAsJsonAsync("/api/v1/banners", payload).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var banner = await db.Banners
            .FirstOrDefaultAsync(
                b => string.Compare(b.Title, "Integrity Test") == 0,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        banner.Should().NotBeNull();
        banner!.ImageUrl.Should().Be("http://anh-em-motor.com/banner.jpg");
    }
}
