using Application.ApiContracts.ManagerChat.Requests;
using Domain.Constants.Permission;
using FluentAssertions;
using IntegrationTests.SetupClass;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Marketing.BannerManagement.Create],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginResponse?.AccessToken);
        var response = await _client.GetAsync("/api/v1/manager-chat/sessions", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "MCHAT_02 - Bị từ chối truy cập nếu không có quyền nào")]
    public async Task GetSessions_ReturnsForbidden_WhenNoPermission()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginResponse?.AccessToken);
        var response = await _client.GetAsync("/api/v1/manager-chat/sessions", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "MCHAT_03 - Tạo mới phiên chat thành công")]
    public async Task CreateSession_ReturnsCreated_WhenHasPermission()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [Permissions.Marketing.BannerManagement.Create],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginResponse?.AccessToken);
        var payload = new CreateManagerChatSessionRequest { Title = "Test Session" };
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/manager-chat/sessions",
            payload,
            TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "MCHAT_04 - Endpoint gửi tin nhắn REST không còn tồn tại")]
    public async Task SendMessageEndpoint_KhongConTonTai()
    {
        var token = await CreateUserAndLoginAsync($"user_{Guid.NewGuid():N}"[..20]).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var created = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/manager-chat/sessions",
            new CreateManagerChatSessionRequest { Title = "Phiên để thử endpoint cũ" },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        created.EnsureSuccessStatusCode();
        var sessionId = (await created.Content
            .ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true)).GetProperty("id")
            .GetGuid();
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            $"/api/v1/manager-chat/sessions/{sessionId}/message",
            new { content = "xin chào" },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact(DisplayName = "MCHAT_05 - Không thể xem lịch sử phiên chat của người khác")]
    public async Task GetHistory_TraVeNotFound_KhiSessionCuaNguoiKhac()
    {
        var tokenA = await CreateUserAndLoginAsync($"userA_{Guid.NewGuid():N}"[..20]).ConfigureAwait(true);
        var tokenB = await CreateUserAndLoginAsync($"userB_{Guid.NewGuid():N}"[..20]).ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var created = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/manager-chat/sessions",
            new CreateManagerChatSessionRequest { Title = "Của A" },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        created.EnsureSuccessStatusCode();
        var sessionId = (await created.Content
            .ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true)).GetProperty("id")
            .GetGuid();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var response = await _client.GetAsync(
            $"/api/v1/manager-chat/sessions/{sessionId}/history",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Tạo user có quyền bất kỳ rồi đăng nhập, trả về access token. `CreateUserWithPermissionsAsync` trả về <see
    /// cref="Domain.Entities.ApplicationUser" /> chứ không phải token, nên vẫn phải gọi thêm `AuthenticateAsync`.
    /// </summary>
    private async Task<string> CreateUserAndLoginAsync(string username)
    {
        const string password = "Password123!";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Permissions.Marketing.BannerManagement.Create],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        return loginResponse.AccessToken ??
            throw new InvalidOperationException($"Không lấy được access token cho {username}");
    }
}
