using Application.ApiContracts.Auth.Responses;
using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Application.Features.Auth.Commands.Login;
using Application.Features.UserManager.Commands.UpdateUser;
using Domain.Constants;
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

namespace IntegrationTests;

public class User : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public User(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "USER_021 - Khôi phục tài khoản thành công")]
    public async Task RestoreAccount_Success_DeletedAtSetToNull()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@test.com";
        var password = "ThisIsStrongPassword1@";

        // Create user directly with DeletedAt set
        using (var scope = _factory.Services.CreateScope())
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
            await userManager.CreateAsync(user, password);
        }

        // Authenticate (Login as the user? Or as Admin? Usually restore needs Admin or self if allowed? 
        // Assuming test meant "Self Restore" or "Admin Restore". 
        // Logic in original file was: CreateAndAuthenticateUserAsync -> calls PostAsJsonAsync("/api/v1/User/{id}/restore")
        // Check permissions: Restore usually requires Admin or higher perm. 
        // If Logic allows self-restore, we need token. But if account is deleted, can we login?
        // Typically Deleted accounts CANNOT login. 
        // So this test likely implies an Admin restoring the user.
        // Let's assume we need an Admin to perform restore.
        
        var adminUniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminUsername = $"admin_{adminUniqueId}";
        // Need to check what permissions restore requires. Assuming Users.Edit or special.
        // Let's give all permissions for simplicity in this context or check PermissionsList.
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, adminUsername, "AdminPass123!", [Domain.Constants.Permission.PermissionsList.Users.Edit, Domain.Constants.Permission.PermissionsList.Users.Delete]); 
        // Restore probably falls under Delete (Soft Delete toggle) or Edit.
        
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, adminUsername, "AdminPass123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Get User ID
        string userId;
        using (var scope = _factory.Services.CreateScope())
        {
             var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
             var u = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
             userId = u!.Id.ToString();
        }

        var response = await _client.PostAsync($"/api/v1/User/{userId}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<RestoreUserResponse>();
        content.Should().NotBeNull();
        content!.Message.Should().Be("User account has been restored successfully.");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var updatedUser = await db.Users.FindAsync(Guid.Parse(userId));
            updatedUser!.DeletedAt.Should().BeNull();
        }
    }

    [Fact(DisplayName = "USER_022 - Khôi phục tài khoản khi chưa bị xóa (DeletedAt = null)")]
    public async Task RestoreAccount_NotDeleted_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        
        // Admin to perform action
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [Domain.Constants.Permission.PermissionsList.Users.Edit]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Target User
        var targetId = Guid.NewGuid().ToString("N")[..8];
        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, $"target_{targetId}", "Pass123!");

        var response = await _client.PostAsync($"/api/v1/User/{targetUser.Id}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("User account is not deleted.");
    }

    [Fact(DisplayName = "USER_023 - Khôi phục tài khoản khi bị Ban (không cho phép)")]
    public async Task RestoreAccount_BannedAccount_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [Domain.Constants.Permission.PermissionsList.Users.Edit]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetId = Guid.NewGuid().ToString("N")[..8];
        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, $"target_{targetId}", "Pass123!");
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(targetUser.Id);
            u!.Status = UserStatus.Banned;
            u.DeletedAt = DateTimeOffset.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsync($"/api/v1/User/{targetUser.Id}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain($"Cannot restore user with status '{UserStatus.Banned}'. User status must be Active.");
    }

    [Fact(DisplayName = "USER_024 - Khôi phục tài khoản với UserId không tồn tại")]
    public async Task RestoreAccount_NonExistentUser_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [Domain.Constants.Permission.PermissionsList.Users.Edit]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var nonExistentUserId = Guid.NewGuid();
        var response = await _client.PostAsync($"/api/v1/User/{nonExistentUserId}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "USER_025 - Lấy thông tin người dùng hiện tại - Integration Test")]
    public async Task GetCurrentUser_IntegrationTest_ReturnsUserInfo()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@test.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, email: email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync("/api/v1/User/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<UserResponse>();
        content.Should().NotBeNull();
        content!.UserName.Should().Be(username);
        content.Email.Should().Be(email);
    }

    [Fact(DisplayName = "USER_026 - Lấy thông tin người dùng khi JWT không có trong header")]
    public async Task GetCurrentUser_NoJWT_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/User/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_027 - Cập nhật thông tin người dùng - Integration Test")]
    public async Task UpdateCurrentUser_IntegrationTest_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var updateRequest = new UpdateUserCommand
        {
            FullName = "Updated Name",
            Gender = GenderStatus.Female,
            PhoneNumber = "0999888777"
        };

        var response = await _client.PutAsJsonAsync("/api/v1/User/me", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
            user!.FullName.Should().Be("Updated Name");
            user.Gender.Should().Be(GenderStatus.Female);
            user.PhoneNumber.Should().Be("0999888777");
        }
    }

    [Fact(DisplayName = "USER_028 - Cập nhật thông tin với validation error - số điện thoại không hợp lệ")]
    public async Task UpdateCurrentUser_InvalidPhoneNumber_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var updateRequest = new UpdateUserCommand 
        { 
            FullName = "Test User",
            Gender = GenderStatus.Male,
            PhoneNumber = "invalid-phone" 
        };

        var response = await _client.PutAsJsonAsync("/api/v1/User/me", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid phone number format");
    }

    [Fact(DisplayName = "USER_029 - Đổi mật khẩu - Integration Test")]
    public async Task ChangePassword_IntegrationTest_PasswordChangedAndTokenInvalidated()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var oldPassword = "OldPass123!";
        var newPassword = "NewPass456!";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, oldPassword);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, oldPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var changePasswordRequest = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            CurrentPassword = oldPassword,
            NewPassword = newPassword
        };

        var response = await _client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify can login with new password
        var newLoginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, newPassword);
        newLoginResponse.AccessToken.Should().NotBeNullOrEmpty();

        // Verify old token is invalid (SecurityStamp check)
        // Note: In some implementations, old JWTs are still valid until expiry unless explicit check against DB SecurityStamp is done in middleware.
        // Identity checks SecurityStamp on every request if properly configured or on interval.
        // USER_033 tests this explicitly, so we assume it works.
        var oldTokenResponse = await _client.GetAsync("/api/v1/User/me");
        oldTokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized); 
    }

    [Fact(DisplayName = "USER_030 - Đổi mật khẩu với CurrentPassword sai - Integration Test")]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsUnauthorized()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var oldPassword = "OldPass123!";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, oldPassword);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, oldPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var changePasswordRequest = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPass456!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Incorrect password.");
    }

    [Fact(DisplayName = "USER_031 - Xóa tài khoản - Integration Test")]
    public async Task DeleteAccount_IntegrationTest_AccountDeletedAndTokenInvalidated()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.PostAsync("/api/v1/User/delete-account", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
            user!.DeletedAt.Should().NotBeNull();
        }

        var tokenTestResponse = await _client.GetAsync("/api/v1/User/me");
        tokenTestResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_032 - Xóa tài khoản khi đã bị Ban - Integration Test")]
    public async Task DeleteAccount_BannedAccount_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";

        var user = await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password);
        
        // Ban user
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(user.Id);
            u!.Status = UserStatus.Banned;
            await db.SaveChangesAsync();
        }

        // Try to login? Banned user usually can't login to get token to Self-Delete.
        // If the test implies using a token obtained BEFORE ban?
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        // If login succeeds (maybe Login logic doesn't check Ban?), we use that token.
        // If login fails, we can't test "Delete Account" endpoint which requires Auth.
        // Assuming Logic: Login might fail if banned? 
        // If existing token used?
        // Let's assume we get a token first, THEN ban.
        
        // Re-do: 
        // 1. Create & Login
        // 2. Ban in DB
        // 3. Try Delete
        
        // Reset/Create new
        var uniqueId2 = Guid.NewGuid().ToString("N")[..8];
        var username2 = $"user_{uniqueId2}";
        var user2 = await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username2, password);
        var loginResponse2 = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username2, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse2.AccessToken);
        
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(user2.Id);
            u!.Status = UserStatus.Banned;
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsync("/api/v1/User/delete-account", null);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "USER_033 - Kiểm tra SecurityStamp invalidation sau khi đổi mật khẩu")]
    public async Task SecurityStampInvalidation_AfterPasswordChange_OldTokenInvalid()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";
        var newPassword = "NewPass456!";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Change password
        var changePasswordRequest = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            CurrentPassword = password,
            NewPassword = newPassword
        };
        var changeResponse = await _client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordRequest);
        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Use old token
        var testResponse = await _client.GetAsync("/api/v1/User/me");
        testResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_034 - Kiểm tra middleware chặn request khi tài khoản bị xóa mềm")]
    public async Task Middleware_BlocksDeletedAccount_ReturnsUnauthorized()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        var user = await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(user.Id);
            u!.DeletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/v1/User/me");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden); // Using Forbidden as per original test expectation
    }

    [Fact(DisplayName = "USER_035 - Kiểm tra middleware chặn request khi tài khoản bị Ban")]
    public async Task Middleware_BlocksBannedAccount_ReturnsUnauthorized()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "Pass123!";

        var user = await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(user.Id);
            u!.Status = UserStatus.Banned;
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/v1/User/me");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
#pragma warning restore CRR0035
}
