using Application.ApiContracts.User.Responses;
using Application.Features.UserManager.Commands.UpdateUser;
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
    public async Task Middleware_BlocksBannedAccount_ReturnsUnauthorized()
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
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
#pragma warning restore CRR0035
}
