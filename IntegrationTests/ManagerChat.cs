using Application.ApiContracts.ManagerChat.Requests;
using Domain.Constants.Permission;
using FluentAssertions;
using IntegrationTests.SetupClass;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

public class ManagerChat : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public ManagerChat(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
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

    [Fact(DisplayName = "MCHAT_01 - Lấy danh sách phiên chat thành công")]
    public async Task GetSessions_ReturnsOk_WhenHasPermission()
    {
        // 1. Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        // ManagerChat chỉ yêu cầu có quyền bất kỳ
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Marketing.BannerManagement.Create],
            CancellationToken.None)
            .ConfigureAwait(true);
            
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!")
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse?.AccessToken);

        // 2. Act
        var response = await _client.GetAsync("/api/v1/manager-chat/sessions");

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "MCHAT_02 - Bị từ chối truy cập nếu không có quyền nào")]
    public async Task GetSessions_ReturnsForbidden_WhenNoPermission()
    {
        // 1. Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [], // Không có quyền nào
            CancellationToken.None)
            .ConfigureAwait(true);
            
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!")
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse?.AccessToken);

        // 2. Act
        var response = await _client.GetAsync("/api/v1/manager-chat/sessions");

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "MCHAT_03 - Tạo mới phiên chat thành công")]
    public async Task CreateSession_ReturnsCreated_WhenHasPermission()
    {
        // 1. Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Marketing.BannerManagement.Create],
            CancellationToken.None)
            .ConfigureAwait(true);
            
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!")
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse?.AccessToken);

        var payload = new CreateManagerChatSessionRequest { Title = "Test Session" };

        // 2. Act
        var response = await _client.PostAsJsonAsync("/api/v1/manager-chat/sessions", payload);

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // HandleResult with Result.Success maps to Ok
    }
}
