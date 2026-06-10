using Application.ApiContracts.Banner.Responses;
using Application.Features.Banners.Commands.UpdateBanner;
using Domain.Constants.Permission.Permissions;
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

    [Fact(DisplayName = "BANN_012 - Kiểm tra tính toàn vẹn của dữ liệu ảnh (DesktopImageUrl)")]
    public async Task CreateBanner_ValidDesktopImageUrl_SavesCorrectly()
    {
        var payload = new { title = "Integrity Test", desktop_image_url = "http://anh-em-motor.com/banner.jpg" };
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            ["Domain.Constants.Permission.Permissions.Banners.Create"],
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
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var banner = await db.Banners
            .FirstOrDefaultAsync(
                b => string.Compare(b.Title, "Integrity Test") == 0,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        banner.Should().NotBeNull();
        banner!.DesktopImageUrl.Should().Be("http://anh-em-motor.com/banner.jpg");
    }

    [Fact(DisplayName = "BANN_024 - Tự động ghi Log khi cập nhật nội dung")]
    public async Task BANN_024_Update_Triggers_AuditLog()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var banner = new Domain.Entities.Banner { Title = "Audit Test", DesktopImageUrl = "OldUrl" };
        db.Banners.Add(banner);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Banners.Edit, Banners.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var payload = new UpdateBannerCommand
        {
            Id = banner.Id,
            Title = "Audit Test",
            DesktopImageUrl = "NewUrl"
        };
        await _client.PutAsJsonAsync($"/api/v1/banners/{banner.Id}", payload).ConfigureAwait(true);
        var auditResponse = await _client.GetAsync($"/api/v1/banners/{banner.Id}/audit", CancellationToken.None)
            .ConfigureAwait(true);
        auditResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
