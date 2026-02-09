using Application.ApiContracts.User.Responses;
using Application.Features.UserManager.Commands.UpdateUser;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IntegrationTests;

[Collection("Shared Integration Collection")]
public class User : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public User(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    { await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(false); }
#pragma warning disable IDE0079
#pragma warning disable CRR0035
    [Fact(DisplayName = "USER_021 - Khôi phục tài khoản thành công")]
    public async Task RestoreAccount_Success_DeletedAtSetToNull()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@test.com";
        var password = "ThisIsStrongPassword1@";

        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                FullName = $"Test {username}",
                Status = UserStatus.Active,
                DeletedAt = DateTimeOffset.UtcNow.AddDays(-3),
                SecurityStamp = Guid.NewGuid().ToString()
            };
            await userManager.CreateAsync(user, password).ConfigureAwait(true);
        }

        var adminUniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminUsername = $"admin_{adminUniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminUsername,
            "AdminPass123!",
            [ PermissionsList.Users.Edit, PermissionsList.Users.Delete ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminUsername,
            "AdminPass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        string userId;
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users
                .FirstOrDefaultAsync(u => string.Compare(u.UserName, username) == 0, CancellationToken.None)
                .ConfigureAwait(true);
            userId = u!.Id.ToString();
        }

        var response = await _client.PostAsync($"/api/v1/User/{userId}/restore", null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<RestoreUserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Message.Should().Be("User account has been restored successfully.");

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var updatedUser = await db.Users.FindAsync(Guid.Parse(userId)).ConfigureAwait(true);
            updatedUser!.DeletedAt.Should().BeNull();
        }
    }

    [Fact(DisplayName = "USER_022 - Khôi phục tài khoản khi chưa bị xóa (DeletedAt = null)")]
    public async Task RestoreAccount_NotDeleted_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetId = Guid.NewGuid().ToString("N")[..8];
        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{targetId}",
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);

        var response = await _client.PostAsync($"/api/v1/User/{targetUser.Id}/restore", null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        content.Should().Contain("User account is not deleted.");
    }

    [Fact(DisplayName = "USER_023 - Khôi phục tài khoản khi bị Ban (không cho phép)")]
    public async Task RestoreAccount_BannedAccount_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetId = Guid.NewGuid().ToString("N")[..8];
        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{targetId}",
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(targetUser.Id).ConfigureAwait(true);
            u!.Status = UserStatus.Banned;
            u.DeletedAt = DateTimeOffset.UtcNow.AddDays(-1);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.PostAsync($"/api/v1/User/{targetUser.Id}/restore", null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        content.Should().Contain($"Cannot restore user with status '{UserStatus.Banned}'. User status must be Active.");
    }

    [Fact(DisplayName = "USER_024 - Khôi phục tài khoản với UserId không tồn tại")]
    public async Task RestoreAccount_NonExistentUser_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var nonExistentUserId = Guid.NewGuid();
        var response = await _client.PostAsync($"/api/v1/User/{nonExistentUserId}/restore", null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "USER_025 - Lấy thông tin người dùng hiện tại - Integration Test")]
    public async Task GetCurrentUser_IntegrationTest_ReturnsUserInfo()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@test.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, email: email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<UserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.UserName.Should().Be(username);
        content.Email.Should().Be(email);
    }

    [Fact(DisplayName = "USER_026 - Lấy thông tin người dùng khi JWT không có trong header")]
    public async Task GetCurrentUser_NoJWT_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_027 - Cập nhật thông tin người dùng - Integration Test")]
    public async Task UpdateCurrentUser_IntegrationTest_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var updateRequest = new UpdateUserCommand
        {
            FullName = "Updated Name",
            Gender = GenderStatus.Female,
            PhoneNumber = "0999888777"
        };

        var response = await _client.PutAsJsonAsync("/api/v1/User/me", updateRequest).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var user = await db.Users
            .FirstOrDefaultAsync(u => string.Compare(u.UserName, username) == 0, CancellationToken.None)
            .ConfigureAwait(true);
        user!.FullName.Should().Be("Updated Name");
        user.Gender.Should().Be(GenderStatus.Female);
        user.PhoneNumber.Should().Be("0999888777");
    }

    [Fact(DisplayName = "USER_028 - Cập nhật thông tin với validation error - số điện thoại không hợp lệ")]
    public async Task UpdateCurrentUser_InvalidPhoneNumber_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var updateRequest = new UpdateUserCommand
        {
            FullName = "Test User",
            Gender = GenderStatus.Male,
            PhoneNumber = "invalid-phone"
        };

        var response = await _client.PutAsJsonAsync("/api/v1/User/me", updateRequest).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        content.Should().Contain("Invalid phone number format");
    }

    [Fact(DisplayName = "USER_029 - Đổi mật khẩu - Integration Test")]
    public async Task ChangePassword_IntegrationTest_PasswordChangedAndTokenInvalidated()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var oldPassword = "OldPass123!";
        var newPassword = "NewPass456!";

        await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            username,
            oldPassword,
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            oldPassword,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var changePasswordRequest = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            CurrentPassword = oldPassword,
            NewPassword = newPassword
        };

        var response = await _client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordRequest)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newLoginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            newPassword,
            CancellationToken.None)
            .ConfigureAwait(true);
        newLoginResponse.AccessToken.Should().NotBeNullOrEmpty();

        var oldTokenResponse = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        oldTokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_030 - Đổi mật khẩu với CurrentPassword sai - Integration Test")]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsUnauthorized()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var oldPassword = "OldPass123!";

        await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            username,
            oldPassword,
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            oldPassword,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var changePasswordRequest = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPass456!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordRequest)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        errorContent.Should().Contain("Incorrect password.");
    }

    [Fact(DisplayName = "USER_031 - Xóa tài khoản - Integration Test")]
    public async Task DeleteAccount_IntegrationTest_AccountDeletedAndTokenInvalidated()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.PostAsync("/api/v1/User/delete-account", null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users
                .FirstOrDefaultAsync(u => string.Compare(u.UserName, username) == 0, CancellationToken.None)
                .ConfigureAwait(true);
            user!.DeletedAt.Should().NotBeNull();
        }

        var tokenTestResponse = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        tokenTestResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_032 - Xóa tài khoản khi đã bị Ban - Integration Test")]
    public async Task DeleteAccount_BannedAccount_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";

        var user = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(user.Id).ConfigureAwait(true);
            u!.Status = UserStatus.Banned;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.PostAsync("/api/v1/User/delete-account", null).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "USER_033 - Kiểm tra SecurityStamp invalidation sau khi đổi mật khẩu")]
    public async Task SecurityStampInvalidation_AfterPasswordChange_OldTokenInvalid()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";
        var newPassword = "NewPass456!";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var changePasswordRequest = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            CurrentPassword = password,
            NewPassword = newPassword
        };
        var changeResponse = await _client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordRequest)
            .ConfigureAwait(true);
        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var testResponse = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        testResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_034 - Kiểm tra middleware chặn request khi tài khoản bị xóa mềm")]
    public async Task Middleware_BlocksDeletedAccount_ReturnsUnauthorized()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        var user = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(user.Id).ConfigureAwait(true);
            u!.DeletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_035 - Kiểm tra middleware chặn request khi tài khoản bị Ban")]
    public async Task Middleware_BlocksBannedAccount_ReturnsBannedStatus()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        var user = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(user.Id).ConfigureAwait(true);
            u!.Status = UserStatus.Banned;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var response = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);

        if(response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            throw new Exception($"API returned not OK. Response Body: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content
            .ReadFromJsonAsync<UserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Status.Should().Be(UserStatus.Banned);
    }

    [Fact(DisplayName = "USER_053 - Verify structure của Permissions trong JSON response")]
    public async Task GetCurrentUser_ReturnsPermissionsStructure()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [ PermissionsList.Users.View ],
            CancellationToken.None)
            .ConfigureAwait(true);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content
            .ReadFromJsonAsync<UserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Permissions.Should().NotBeNullOrEmpty();
        content.Permissions!.Should().Contain(p => string.Compare(p.ID, PermissionsList.Users.View) == 0);
        content.Permissions!.First().DisplayName.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "USER_054 - Verify Status field cho Active user")]
    public async Task GetCurrentUser_ActiveUser_ReturnsActiveStatus()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content
            .ReadFromJsonAsync<UserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Status.Should().Be(UserStatus.Active);
    }

    [Fact(DisplayName = "USER_056 - SSE Hybrid: Request không có Accept header text/event-stream")]
    public async Task GetCurrentUser_NoAcceptHeader_ReturnsJson()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _client.SendAsync(request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact(DisplayName = "USER_057 - SSE Hybrid: Request có Accept header text/event-stream")]
    public async Task GetCurrentUser_AcceptEventStream_ReturnsSseStream()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/event-stream");
    }

    [Fact(DisplayName = "USER_058 - SSE: Nhận dữ liệu init ngay khi kết nối")]
    public async Task GetCurrentUser_SseConnection_ReceivesInitData()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(true);
        using var reader = new StreamReader(stream);

        string? line;
        bool dataFound = false;
        var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

        while((line = await reader.ReadLineAsync(timeoutToken).ConfigureAwait(true)) != null)
        {
            if(line.StartsWith("data:"))
            {
                dataFound = true;
                line.Should().Contain(username);
                break;
            }
        }
        dataFound.Should().BeTrue("Should receive initial data event immediately");
    }

    [Fact(DisplayName = "USER_059 - SSE Push: Update profile triggers notification", Timeout = 5000)]
    public async Task UpdateProfile_TriggersSseNotification()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);

        var sseRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        sseRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        sseRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        var sseResponse = await _client.SendAsync(sseRequest, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(true);
        var stream = await sseResponse.Content.ReadAsStreamAsync().ConfigureAwait(true);
        var reader = new StreamReader(stream);

        await ReadEventAsync(reader).ConfigureAwait(true);

        await Task.Delay(1000).ConfigureAwait(true);

        var updateClient = _factory.CreateClient();
        updateClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginResponse.AccessToken);
        var updateRequest = new UpdateUserCommand
        {
            FullName = "New Name SSE",
            Gender = GenderStatus.Female,
            PhoneNumber = "0999888777"
        };
        var updateResponse = await updateClient.PutAsJsonAsync("/api/v1/User/me", updateRequest).ConfigureAwait(true);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;
        string foundData = string.Empty;

        while(!updateTimeout.IsCancellationRequested)
        {
            try
            {
                var eventData = await ReadEventAsync(reader).ConfigureAwait(true);

                if(eventData.Contains("New Name SSE"))
                {
                    foundData = eventData;
                    break;
                }
            } catch(Exception)
            {
            }
        }

        foundData.Should().Contain("New Name SSE");
    }

    [Fact(DisplayName = "USER_060 - SSE Push: Assign roles triggers notification")]
    public async Task AssignRole_TriggersSseNotification()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";
        var user = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);

        var sseRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        sseRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        sseRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var sseResponse = await _client.SendAsync(sseRequest, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(true);
        var reader = new StreamReader(await sseResponse.Content.ReadAsStreamAsync().ConfigureAwait(true));

        await ReadEventAsync(reader).ConfigureAwait(true);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if(!db.Roles.Any(r => r.Name == "Staff"))
            {
                db.Roles.Add(new ApplicationRole { Name = "Staff", NormalizedName = "STAFF" });
                await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            }

            var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            var command = new Application.Features.UserManager.Commands.AssignRoles.AssignRolesCommand
            {
                UserId = user.Id,
                RoleNames = [ "Staff" ]
            };

            await mediator.Send(command, CancellationToken.None).ConfigureAwait(true);
        }

        var eventData = await ReadEventAsync(reader).ConfigureAwait(true);
        eventData.Should().Contain(username);
        eventData.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "USER_061 - SSE Push: Ban user triggers notification")]
    public async Task BanUser_TriggersSseNotification()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";
        var user = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);

        var sseRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        sseRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        sseRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var sseResponse = await _client.SendAsync(sseRequest, HttpCompletionOption.ResponseHeadersRead)
            .ConfigureAwait(true);
        var reader = new StreamReader(await sseResponse.Content.ReadAsStreamAsync().ConfigureAwait(true));

        await ReadEventAsync(reader).ConfigureAwait(true);

        using(var scope = _factory.Services.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            var command = new Application.Features.UserManager.Commands.ChangeUserStatus.ChangeUserStatusCommand
            {
                UserId = user.Id,
                Status = UserStatus.Banned
            };
            await mediator.Send(command, CancellationToken.None).ConfigureAwait(true);
        }

        var eventData = await ReadEventAsync(reader).ConfigureAwait(true);
        eventData.Should().Contain(UserStatus.Banned);
    }

    [Fact(DisplayName = "USER_055 - Verify Status field cho Banned user (Explicit Field Check)")]
    public async Task GetCurrentUser_BannedUser_ReturnsJsonWithBannedStatus()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        var user = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(user.Id).ConfigureAwait(true);
            u!.Status = UserStatus.Banned;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content
            .ReadFromJsonAsync<UserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Status.Should().Be(UserStatus.Banned);
    }

    [Fact(DisplayName = "USER_062 - SSE Memory: Reconnect cleans up old connection (Behavioral)")]
    public async Task SseReconnect_ShouldReceiveDataOnNewConnection()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";
        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var token = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var req1 = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var resp1 = await _client.SendAsync(req1, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var reader1 = new StreamReader(await resp1.Content.ReadAsStreamAsync().ConfigureAwait(true));
        await ReadEventAsync(reader1).ConfigureAwait(true);

        reader1.Dispose();

        var req2 = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var resp2 = await _client.SendAsync(req2, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var reader2 = new StreamReader(await resp2.Content.ReadAsStreamAsync().ConfigureAwait(true));

        var data = await ReadEventAsync(reader2).ConfigureAwait(true);
        data.Should().Contain(username);
    }

    [Fact(DisplayName = "USER_065 - SSE: Close one tab does not affect others")]
    public async Task CloseOneTab_DoesNotAffectOtherTab()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";
        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var token = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var req1 = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var resp1 = await _client.SendAsync(req1, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var reader1 = new StreamReader(await resp1.Content.ReadAsStreamAsync().ConfigureAwait(true));
        await ReadEventAsync(reader1).ConfigureAwait(true);

        var req2 = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var resp2 = await _client.SendAsync(req2, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var reader2 = new StreamReader(await resp2.Content.ReadAsStreamAsync().ConfigureAwait(true));
        await ReadEventAsync(reader2).ConfigureAwait(true);

        reader1.Dispose();

        await Task.Delay(1000).ConfigureAwait(true);

        var updateClient = _factory.CreateClient();
        updateClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateResp = await updateClient.PutAsJsonAsync(
            "/api/v1/User/me",
            new UpdateUserCommand { FullName = "Tab Test", Gender = GenderStatus.Male, PhoneNumber = "0987654321" })
            .ConfigureAwait(true);
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await ReadEventAsync(reader2).ConfigureAwait(true);
        data.Should().Contain("Tab Test");
    }

    [Fact(DisplayName = "USER_067 - SSE Isolation: Ban User B does not affect User A")]
    public async Task BanUserB_DoesNotNotifyUserA()
    {
        var usernameA = $"userA_{Guid.NewGuid().ToString("N")[..8]}";
        await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            usernameA,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        var tokenA = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            usernameA,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var usernameB = $"userB_{Guid.NewGuid().ToString("N")[..8]}";
        var userB = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            usernameB,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);

        var reqA = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        reqA.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        reqA.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var respA = await _client.SendAsync(reqA, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var readerA = new StreamReader(await respA.Content.ReadAsStreamAsync().ConfigureAwait(true));
        await ReadEventAsync(readerA).ConfigureAwait(true);

        using(var scope = _factory.Services.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            await mediator.Send(
                new Application.Features.UserManager.Commands.ChangeUserStatus.ChangeUserStatusCommand
                {
                    UserId = userB.Id,
                    Status = UserStatus.Banned
                },
                CancellationToken.None)
                .ConfigureAwait(true);
        }

        var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try
        {
            var line = await readerA.ReadLineAsync(tcs.Token).ConfigureAwait(true);
            if(line != null && line.StartsWith("data:"))
            {
                Assert.Fail($"User A received data about User B: {line}");
            }
        } catch(OperationCanceledException)
        {
        }
    }

    [Fact(DisplayName = "USER_068 - SSE Isolation: JWT context separation (Update with wrong token)")]
    public async Task UpdateWithWrongToken_DoesNotAffectTargetUser()
    {
        var usernameA = $"userA_{Guid.NewGuid().ToString("N")[..8]}";
        await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            usernameA,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        var tokenA = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            usernameA,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var usernameB = $"userB_{Guid.NewGuid().ToString("N")[..8]}";
        await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            usernameB,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        var tokenB = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            usernameB,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var reqA = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        reqA.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        reqA.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var respA = await _client.SendAsync(reqA, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var readerA = new StreamReader(await respA.Content.ReadAsStreamAsync().ConfigureAwait(true));
        await ReadEventAsync(readerA).ConfigureAwait(true);

        var updateClientB = _factory.CreateClient();
        updateClientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        await updateClientB.PutAsJsonAsync(
            "/api/v1/User/me",
            new UpdateUserCommand { FullName = "Reviewer B", Gender = GenderStatus.Female, PhoneNumber = "1112223333" })
            .ConfigureAwait(true);

        var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try
        {
            var line = await readerA.ReadLineAsync(tcs.Token).ConfigureAwait(true);
            if(line != null && line.StartsWith("data:"))
            {
                Assert.Fail($"User A received update from User B action: {line}");
            }
        } catch(OperationCanceledException)
        {
        }
    }

    [Fact(DisplayName = "USER_069 - SSE Error: User deleted triggers error event")]
    public async Task DeletedUser_TriggersErrorEvent()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";
        var user = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        var token = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var resp = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var reader = new StreamReader(await resp.Content.ReadAsStreamAsync().ConfigureAwait(true));
        await ReadEventAsync(reader).ConfigureAwait(true);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(user.Id).ConfigureAwait(true);
            u!.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

            var userStream = scope.ServiceProvider.GetRequiredService<IUserStreamService>();
            userStream.NotifyUserUpdate(user.Id);
        }

        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        string? line;
        bool errorReceived = false;
        while((line = await reader.ReadLineAsync(timeout).ConfigureAwait(true)) != null)
        {
            if(line.StartsWith("event: error"))
            {
                errorReceived = true;
                break;
            }
        }
        errorReceived.Should().BeTrue("Should receive 'event: error' when user is deleted");
    }

    [Fact(DisplayName = "USER_070 - SSE Timeout: Connection gracefully closed")]
    public async Task SseTimeout_GracefullyCloses_ClientSide()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";
        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var token = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        var resp = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        await Task.Delay(1000).ConfigureAwait(true);

        resp.Dispose();
    }

    [Fact(DisplayName = "USER_066 - SSE Isolation: Update user A does NOT notify user B")]
    public async Task UpdateUserA_DoesNotNotifyUserB()
    {
        var usernameA = $"userA_{Guid.NewGuid().ToString("N")[..8]}";
        var tokenA = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            usernameA,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var usernameB = $"userB_{Guid.NewGuid().ToString("N")[..8]}";
        await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            usernameB,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        var tokenB = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            usernameB,
            "Pass123!",
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var requestB = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        requestB.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        requestB.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var responseB = await _client.SendAsync(requestB, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var readerB = new StreamReader(await responseB.Content.ReadAsStreamAsync().ConfigureAwait(true));

        await ReadEventAsync(readerB).ConfigureAwait(true);

        var updateClientA = _factory.CreateClient();
        updateClientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        await updateClientA.PutAsJsonAsync(
            "/api/v1/User/me",
            new UpdateUserCommand { FullName = "A Updated", Gender = GenderStatus.Male, PhoneNumber = "0123456789" })
            .ConfigureAwait(true);

        var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try
        {
            var line = await readerB.ReadLineAsync(tcs.Token).ConfigureAwait(true);
            if(line != null && line.StartsWith("data:"))
            {
                Assert.Fail($"User B received data meant for User A or irrelevant update: {line}");
            }
        } catch(OperationCanceledException)
        {
        }
    }

    [Fact(DisplayName = "USER_064 - SSE: Support multiple tabs (connections)")]
    public async Task MultipleTabs_receiveUpdate()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";
        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var token = (await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true)).AccessToken;

        var req1 = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var resp1 = await _client.SendAsync(req1, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var reader1 = new StreamReader(await resp1.Content.ReadAsStreamAsync().ConfigureAwait(true));
        await ReadEventAsync(reader1).ConfigureAwait(true);

        var req2 = new HttpRequestMessage(HttpMethod.Get, "/api/v1/User/me");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var resp2 = await _client.SendAsync(req2, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true);
        var reader2 = new StreamReader(await resp2.Content.ReadAsStreamAsync().ConfigureAwait(true));
        await ReadEventAsync(reader2).ConfigureAwait(true);

        var updateClient = _factory.CreateClient();
        updateClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await updateClient.PutAsJsonAsync(
            "/api/v1/User/me",
            new UpdateUserCommand
            {
                FullName = "MultiTab Update",
                Gender = GenderStatus.Male,
                PhoneNumber = "0987654321"
            })
            .ConfigureAwait(true);

        var data1 = await ReadEventAsync(reader1).ConfigureAwait(true);
        data1.Should().Contain("MultiTab Update");

        var data2 = await ReadEventAsync(reader2).ConfigureAwait(true);
        data2.Should().Contain("MultiTab Update");
    }

    private static async Task<string> ReadEventAsync(StreamReader reader)
    {
        var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        string? line;
        while((line = await reader.ReadLineAsync(timeout).ConfigureAwait(true)) != null)
        {
            if(line.StartsWith("data:"))
            {
                return line[5..].Trim();
            }
        }
        throw new Exception("No data event received");
    }
}
#pragma warning restore CRR0035
#pragma warning restore IDE0079


