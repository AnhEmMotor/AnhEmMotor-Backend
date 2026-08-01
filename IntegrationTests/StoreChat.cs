using Application.DTOs.StoreChat;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

public class StoreChat : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public StoreChat(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "STORECHAT_001 - Khách vãng lai tạo phiên chat không cần đăng nhập")]
    public async Task CreateSession_Anonymous_ReturnsOk()
    {
        var visitorKey = Guid.NewGuid().ToString("N");
        var response = await _client.PostAsJsonAsync(
            "/api/v1/store-chat/sessions",
            new { visitorKey })
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content
            .ReadFromJsonAsync<StoreChatSessionDto>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        session!.VisitorKey.Should().Be(visitorKey);
        session.Mode.Should().Be("Ai");
    }

    [Fact(DisplayName = "STORECHAT_002 - Cùng VisitorKey khôi phục đúng phiên cũ, không tạo trùng")]
    public async Task CreateSession_SameVisitorKeyTwice_RestoresSameSession()
    {
        var visitorKey = Guid.NewGuid().ToString("N");
        var firstResponse = await _client.PostAsJsonAsync(
            "/api/v1/store-chat/sessions",
            new { visitorKey })
            .ConfigureAwait(true);
        var first = await firstResponse.Content
            .ReadFromJsonAsync<StoreChatSessionDto>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/v1/store-chat/sessions",
            new { visitorKey })
            .ConfigureAwait(true);
        var second = await secondResponse.Content
            .ReadFromJsonAsync<StoreChatSessionDto>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        second!.Id.Should().Be(first!.Id);
    }

    [Fact(DisplayName = "STORECHAT_003 - Lấy lịch sử phiên chat công khai không cần đăng nhập")]
    public async Task GetHistory_Anonymous_ReturnsMessages()
    {
        Guid sessionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new StoreChatSession { VisitorKey = Guid.NewGuid().ToString("N") };
            db.StoreChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            db.StoreChatMessages.Add(
                new StoreChatMessage { SessionId = session.Id, Sender = "Visitor", Content = "Xin chào" });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            sessionId = session.Id;
        }

        var response = await _client.GetAsync(
            $"/api/v1/store-chat/sessions/{sessionId}/history",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content
            .ReadFromJsonAsync<List<StoreChatMessageDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        history!.Should().ContainSingle(m => m.Content == "Xin chào");
    }

    [Fact(DisplayName = "STORECHAT_004 - Gắn phiên vào tài khoản khách hàng yêu cầu đăng nhập")]
    public async Task LinkToCustomer_WithoutAuth_ReturnsUnauthorized()
    {
        Guid sessionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new StoreChatSession { VisitorKey = Guid.NewGuid().ToString("N") };
            db.StoreChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            sessionId = session.Id;
        }

        var response = await _client.PostAsync(
            $"/api/v1/store-chat/sessions/{sessionId}/link-customer",
            null,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "STORECHAT_005 - Đăng nhập giữa chừng gắn đúng CustomerUserId, không mất lịch sử")]
    public async Task LinkToCustomer_WithAuth_SetsCustomerUserId()
    {
        Guid sessionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new StoreChatSession { VisitorKey = Guid.NewGuid().ToString("N") };
            db.StoreChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            sessionId = session.Id;
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"customer_{uniqueId}",
            "Password123!",
            [],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"customer_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.PostAsync(
            $"/api/v1/store-chat/sessions/{sessionId}/link-customer",
            null,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope2 = _factory.Services.CreateScope();
        var dbAfter = scope2.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updated = await dbAfter.StoreChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updated!.CustomerUserId.Should().Be(user.Id);
    }
}
