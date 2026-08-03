using Application.DTOs.StoreChat;
using Domain.Constants;
using Domain.Constants.Permission;
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

    [Fact(DisplayName = "STORECHAT_009 - Trả lại AI reset Mode và AssignedStaffId")]
    public async Task Release_HumanSession_ResetsToAi()
    {
        Guid sessionId;
        var staffId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new StoreChatSession
            {
                VisitorKey = Guid.NewGuid().ToString("N"),
                Mode = StoreChatMode.Human,
                AssignedStaffId = staffId
            };
            db.StoreChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            sessionId = session.Id;
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_release_{uniqueId}", "Password123!",
            [Permissions.Marketing.StoreChatManagement.View, Permissions.Marketing.StoreChatManagement.Claim],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_release_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.PostAsync(
            $"/api/v1/store-chat-handoff/sessions/{sessionId}/release", null, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope2 = _factory.Services.CreateScope();
        var dbAfter = scope2.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updated = await dbAfter.StoreChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updated!.Mode.Should().Be(StoreChatMode.Ai);
        updated.AssignedStaffId.Should().BeNull();
    }

    [Fact(DisplayName = "STORECHAT_010 - Danh sách phiên quản trị bị từ chối 403 nếu không có quyền View")]
    public async Task GetSessions_WithoutPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_list_noperm_{uniqueId}", "Password123!", [], TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_list_noperm_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync(
            "/api/v1/store-chat-handoff/sessions", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "STORECHAT_011 - Danh sách phiên quản trị trả đúng phiên đang chờ")]
    public async Task GetSessions_WithPermission_ReturnsWaitingSession()
    {
        Guid sessionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new StoreChatSession
            {
                VisitorKey = Guid.NewGuid().ToString("N"),
                Mode = StoreChatMode.Waiting,
                ContactName = "Khách C",
                ContactPhone = "0922222222"
            };
            db.StoreChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            sessionId = session.Id;
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_list_{uniqueId}", "Password123!",
            [Permissions.Marketing.StoreChatManagement.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_list_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync(
            "/api/v1/store-chat-handoff/sessions", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessions = await response.Content
            .ReadFromJsonAsync<List<StoreChatSessionListItemDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        sessions!.Should().ContainSingle(s => s.Id == sessionId && s.ContactName == "Khách C");
    }

    [Fact(DisplayName = "STORECHAT_022 - Danh sách phiên bỏ thẻ HTML của tin nhắn Staff (rich-text) khỏi preview")]
    public async Task GetSessions_LastMessageIsStaffHtml_PreviewStripsHtmlTags()
    {
        Guid sessionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new StoreChatSession { VisitorKey = Guid.NewGuid().ToString("N") };
            db.StoreChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            db.StoreChatMessages.Add(new StoreChatMessage
            {
                SessionId = session.Id, Sender = StoreChatSender.Staff,
                Content = "<p>Dạ shop còn <strong>xe SH 2024</strong> ạ</p>"
            });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            sessionId = session.Id;
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_preview_{uniqueId}", "Password123!",
            [Permissions.Marketing.StoreChatManagement.View], TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_preview_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync(
            "/api/v1/store-chat-handoff/sessions", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessions = await response.Content
            .ReadFromJsonAsync<List<StoreChatSessionListItemDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var found = sessions.Should().ContainSingle(s => s.Id == sessionId).Subject;
        found.LastMessagePreview.Should().Be("Dạ shop còn xe SH 2024 ạ");
    }

    [Fact(DisplayName = "STORECHAT_017 - Khách đã đăng nhập hiện đúng tên tài khoản, không còn là Khách vãng lai")]
    public async Task GetSessions_CustomerLinkedSession_ReturnsCustomerName()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var customer = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"customer_named_{uniqueId}", "Password123!", [], TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        Guid sessionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new StoreChatSession
            {
                VisitorKey = Guid.NewGuid().ToString("N"),
                Mode = StoreChatMode.Waiting,
                CustomerUserId = customer.Id
            };
            db.StoreChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            sessionId = session.Id;
        }

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_customername_{uniqueId}", "Password123!",
            [Permissions.Marketing.StoreChatManagement.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_customername_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync(
            "/api/v1/store-chat-handoff/sessions", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessions = await response.Content
            .ReadFromJsonAsync<List<StoreChatSessionListItemDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        sessions!.Should().ContainSingle(s => s.Id == sessionId && s.CustomerName == customer.FullName);
    }

    [Fact(DisplayName = "STORECHAT_012 - Endpoint công khai không bị cấp quyền cao hơn khi kèm JWT nhân viên không liên quan")]
    public async Task CreateSession_WithUnrelatedStaffJwt_BehavesLikeAnonymous()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_unrelated_{uniqueId}", "Password123!", [], TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_unrelated_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

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

    [Fact(DisplayName = "STORECHAT_013 - Endpoint công khai vẫn hoạt động bình thường dù Authorization header rác")]
    public async Task CreateSession_WithMalformedAuthHeader_StillSucceeds()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "khong-phai-jwt-hop-le");

        var visitorKey = Guid.NewGuid().ToString("N");
        var response = await _client.PostAsJsonAsync(
            "/api/v1/store-chat/sessions",
            new { visitorKey })
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "STORECHAT_014 - Điền Tên/SĐT hợp lệ trước khi chat lưu đúng vào phiên")]
    public async Task SetContactInfo_ValidNameAndPhone_PersistsOnSession()
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

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/store-chat/sessions/{sessionId}/contact-info",
            new { contactName = "Nguyễn Văn A", contactPhone = "0901234567" })
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope2 = _factory.Services.CreateScope();
        var dbAfter = scope2.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updated = await dbAfter.StoreChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updated!.ContactName.Should().Be("Nguyễn Văn A");
        updated.ContactPhone.Should().Be("0901234567");
    }

    [Fact(DisplayName = "STORECHAT_015 - SĐT không hợp lệ bị từ chối 400")]
    public async Task SetContactInfo_InvalidPhone_ReturnsBadRequest()
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

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/store-chat/sessions/{sessionId}/contact-info",
            new { contactName = "Nguyễn Văn A", contactPhone = "091234" })
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "STORECHAT_016 - Xoá cuộc trò chuyện tạo phiên mới liên kết phiên cũ, phiên cũ giữ nguyên lịch sử cho quản trị")]
    public async Task ClearChat_CreatesNewLinkedSession_OldSessionHistoryUntouched()
    {
        Guid oldSessionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new StoreChatSession
            {
                VisitorKey = Guid.NewGuid().ToString("N"), ContactName = "Khách D", ContactPhone = "0933333333"
            };
            db.StoreChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            db.StoreChatMessages.Add(
                new StoreChatMessage { SessionId = session.Id, Sender = "Visitor", Content = "Tin nhắn cũ" });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            oldSessionId = session.Id;
        }

        // Khách bấm "Xoá cuộc trò chuyện" — Store tạo VisitorKey mới rồi gọi lại chính endpoint tạo/khôi phục
        // phiên, kèm previousSessionId.
        var newVisitorKey = Guid.NewGuid().ToString("N");
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/store-chat/sessions",
            new { visitorKey = newVisitorKey, previousSessionId = oldSessionId })
            .ConfigureAwait(true);

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newSession = await createResponse.Content
            .ReadFromJsonAsync<StoreChatSessionDto>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        newSession!.Id.Should().NotBe(oldSessionId);
        newSession.ContactName.Should().Be("Khách D");
        newSession.ContactPhone.Should().Be("0933333333");

        var newHistoryResponse = await _client.GetAsync(
            $"/api/v1/store-chat/sessions/{newSession.Id}/history", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var newHistory = await newHistoryResponse.Content
            .ReadFromJsonAsync<List<StoreChatMessageDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        newHistory.Should().BeEmpty();

        var oldHistoryResponse = await _client.GetAsync(
            $"/api/v1/store-chat/sessions/{oldSessionId}/history", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var oldHistory = await oldHistoryResponse.Content
            .ReadFromJsonAsync<List<StoreChatMessageDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        oldHistory!.Should().ContainSingle(m => m.Content == "Tin nhắn cũ");

        using var scope2 = _factory.Services.CreateScope();
        var dbAfter = scope2.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updated = await dbAfter.StoreChatSessions
            .FirstOrDefaultAsync(s => s.Id == newSession.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updated!.PreviousSessionId.Should().Be(oldSessionId);
    }

    [Fact(DisplayName = "STORECHAT_017 - Xoá phiên bị từ chối 403 nếu không có quyền Delete")]
    public async Task DeleteSession_WithoutPermission_ReturnsForbidden()
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_delnoperm_{uniqueId}", "Password123!",
            [Permissions.Marketing.StoreChatManagement.View], TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_delnoperm_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.DeleteAsync(
            $"/api/v1/store-chat-handoff/sessions/{sessionId}", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "STORECHAT_018 - Xoá phiên có quyền Delete ẩn khỏi mọi truy vấn (xoá mềm) nhưng vẫn giữ lại cả phiên lẫn tin nhắn")]
    public async Task DeleteSession_WithPermission_SoftDeletesSessionAndMessages()
    {
        Guid sessionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var session = new StoreChatSession { VisitorKey = Guid.NewGuid().ToString("N") };
            db.StoreChatSessions.Add(session);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            db.StoreChatMessages.Add(
                new StoreChatMessage { SessionId = session.Id, Sender = "Visitor", Content = "Xoá tôi đi" });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            sessionId = session.Id;
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_del_{uniqueId}", "Password123!",
            [Permissions.Marketing.StoreChatManagement.Delete], TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_del_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.DeleteAsync(
            $"/api/v1/store-chat-handoff/sessions/{sessionId}", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope2 = _factory.Services.CreateScope();
        var dbAfter = scope2.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Truy vấn thường (có query filter global DeletedAt == null) phải không còn thấy — đúng hành
        // vi "ẩn khỏi trang quản trị" mà tính năng cần.
        (await dbAfter.StoreChatSessions.AnyAsync(s => s.Id == sessionId, TestContext.Current.CancellationToken)
            .ConfigureAwait(true)).Should().BeFalse();
        (await dbAfter.StoreChatMessages.AnyAsync(m => m.SessionId == sessionId, TestContext.Current.CancellationToken)
            .ConfigureAwait(true)).Should().BeFalse();

        // Bỏ qua query filter để xác nhận đây là XOÁ MỀM thật sự — row vẫn còn nguyên trong DB, có
        // DeletedAt, không phải xoá vật lý.
        var sessionIgnoringFilter = await dbAfter.StoreChatSessions.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == sessionId, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        sessionIgnoringFilter.Should().NotBeNull();
        sessionIgnoringFilter!.DeletedAt.Should().NotBeNull();

        var messageIgnoringFilter = await dbAfter.StoreChatMessages.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.SessionId == sessionId, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        messageIgnoringFilter.Should().NotBeNull();
        messageIgnoringFilter!.DeletedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "STORECHAT_019 - Khách quay lại với VisitorKey của phiên đã bị xoá mềm vẫn tạo được phiên mới")]
    public async Task CreateSession_VisitorKeyBelongsToSoftDeletedSession_CreatesNewSessionInstead()
    {
        var visitorKey = Guid.NewGuid().ToString("N");
        Guid oldSessionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var oldSession = new StoreChatSession { VisitorKey = visitorKey };
            db.StoreChatSessions.Add(oldSession);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            oldSessionId = oldSession.Id;
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_del2_{uniqueId}", "Password123!",
            [Permissions.Marketing.StoreChatManagement.Delete], TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_del2_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        (await _client.DeleteAsync(
            $"/api/v1/store-chat-handoff/sessions/{oldSessionId}", TestContext.Current.CancellationToken)
            .ConfigureAwait(true)).StatusCode.Should().Be(HttpStatusCode.OK);
        _client.DefaultRequestHeaders.Authorization = null;

        // Trình duyệt khách vẫn giữ VisitorKey cũ trong localStorage — trước bản vá này, insert sẽ
        // đụng unique index (row cũ vẫn còn trong DB do xoá mềm) và trả 500.
        var response = await _client.PostAsJsonAsync(
            "/api/v1/store-chat/sessions",
            new { visitorKey })
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content
            .ReadFromJsonAsync<StoreChatSessionDto>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        session!.VisitorKey.Should().Be(visitorKey);
        session.Id.Should().NotBe(oldSessionId);
    }

    [Fact(DisplayName = "STORECHAT_020 - Tìm sản phẩm để gán vào tin nhắn bị từ chối 403 nếu không có quyền Claim")]
    public async Task SearchProducts_WithoutClaimPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_prodnoperm_{uniqueId}", "Password123!",
            [Permissions.Marketing.StoreChatManagement.View], TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_prodnoperm_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync(
            "/api/v1/store-chat-handoff/products/search?keyword=abc", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "STORECHAT_021 - Nhân viên có quyền Claim tìm sản phẩm và xem biến thể + màu để gán vào tin nhắn")]
    public async Task SearchProductsAndGetVariants_WithPermission_ReturnsProductWithColors()
    {
        int productId;
        int variantId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            const string statusId = Domain.Constants.Product.ProductStatus.ForSale;
            if (!await db.ProductStatuses.AnyAsync(s => s.Key == statusId, TestContext.Current.CancellationToken)
                .ConfigureAwait(true))
            {
                db.ProductStatuses.Add(new ProductStatus { Key = statusId });
            }
            var category = new Domain.Entities.ProductCategory { Name = $"Category_{suffix}" };
            var brand = new Domain.Entities.Brand { Name = $"Brand_{suffix}" };
            db.ProductCategories.Add(category);
            db.Brands.Add(brand);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

            var product = new Domain.Entities.Product
            {
                Name = $"Honda SH {suffix}", CategoryId = category.Id, BrandId = brand.Id, StatusId = statusId
            };
            db.Products.Add(product);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

            var variant = new ProductVariant
            {
                ProductId = product.Id, VariantName = "Đỏ đen", SKU = $"SKU_{suffix}",
                Price = 91000000, UrlSlug = $"sh-{suffix}"
            };
            db.ProductVariants.Add(variant);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

            db.ProductVariantColors.Add(new ProductVariantColor
            {
                ProductVariantId = variant.Id, ColorName = "Đỏ đen", ColorCode = "#c00", CoverImageUrl = "red.jpg"
            });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

            productId = product.Id;
            variantId = variant.Id;
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, $"staff_prodok_{uniqueId}", "Password123!",
            [Permissions.Marketing.StoreChatManagement.Claim], TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client, $"staff_prodok_{uniqueId}", "Password123!", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var searchResponse = await _client.GetAsync(
            $"/api/v1/store-chat-handoff/products/search?keyword=Honda SH", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchItems = await searchResponse.Content
            .ReadFromJsonAsync<List<StoreChatProductSearchItemDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        searchItems.Should().Contain(i => i.ProductId == productId);

        var variantsResponse = await _client.GetAsync(
            $"/api/v1/store-chat-handoff/products/{productId}/variants", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        variantsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var variants = await variantsResponse.Content
            .ReadFromJsonAsync<List<StoreChatVariantCardDto>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var returnedVariant = variants.Should().ContainSingle(v => v.VariantId == variantId).Subject;
        returnedVariant.Colors.Should().ContainSingle(c => c.ColorName == "Đỏ đen" && c.ColorCode == "#c00");
    }
}
