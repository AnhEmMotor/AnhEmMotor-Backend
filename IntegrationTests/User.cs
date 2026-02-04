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

    private async Task<(ApplicationUser user, string token)> CreateAndAuthenticateUserAsync(
        string username,
        string email,
        string password,
        string status = UserStatus.Active,
        DateTimeOffset? deletedAt = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FullName = $"Test {username}",
            PhoneNumber = "0123456789",
            Gender = GenderStatus.Male,
            Status = status,
            DeletedAt = deletedAt,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        await userManager.CreateAsync(user, password).ConfigureAwait(true);

        var loginRequest = new LoginCommand { UsernameOrEmail = username, Password = password };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginRequest).ConfigureAwait(true);
        var loginResult = await loginResponse.Content
            .ReadFromJsonAsync<LoginResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        return (user, loginResult!.AccessToken!);
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "USER_021 - Khôi phục tài khoản thành công")]
    public async Task RestoreAccount_Success_DeletedAtSetToNull()
    {
        var (user, _) = await CreateAndAuthenticateUserAsync(
            "user021",
            "user021@test.com",
            "Pass123!",
            UserStatus.Active,
            DateTimeOffset.UtcNow.AddDays(-3), CancellationToken.None)
            .ConfigureAwait(true);

        var response = await _client.PostAsync($"/api/v1/User/{user!.Id}/restore", null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<RestoreUserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Message.Should().Be("Account restored successfully");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updatedUser = await db.Users.FindAsync(user!.Id).ConfigureAwait(true);
        updatedUser.Should().NotBeNull();
        updatedUser!.DeletedAt.Should().BeNull();
    }

    [Fact(DisplayName = "USER_022 - Khôi phục tài khoản khi chưa bị xóa (DeletedAt = null)")]
    public async Task RestoreAccount_NotDeleted_ReturnsBadRequest()
    {
        var (user, _) = await CreateAndAuthenticateUserAsync(
            "user022",
            "user022@test.com",
            "Pass123!",
            UserStatus.Active,
            null, CancellationToken.None)
            .ConfigureAwait(true);

        var response = await _client.PostAsync($"/api/v1/User/{user!.Id}/restore", null).ConfigureAwait(true);

        response!.StatusCode!.Should().Be(HttpStatusCode.BadRequest);
        var content = await response!.Content!.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        content.Should().Contain("Account is not deleted");
    }

    [Fact(DisplayName = "USER_023 - Khôi phục tài khoản khi bị Ban (không cho phép)")]
    public async Task RestoreAccount_BannedAccount_ReturnsForbidden()
    {
        var (user, _) = await CreateAndAuthenticateUserAsync(
            "user023",
            "user023@test.com",
            "Pass123!",
            UserStatus.Banned,
            DateTimeOffset.UtcNow.AddDays(-5), CancellationToken.None)
            .ConfigureAwait(true);

        var response = await _client.PostAsync($"/api/v1/User/{user!.Id}/restore", null).ConfigureAwait(true);

        response!.StatusCode!.Should().Be(HttpStatusCode.Forbidden);
        var content = await response!.Content!.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        content.Should().Contain("Cannot restore banned account");
    }

    [Fact(DisplayName = "USER_024 - Khôi phục tài khoản với UserId không tồn tại")]
    public async Task RestoreAccount_NonExistentUser_ReturnsNotFound()
    {
        var nonExistentUserId = Guid.NewGuid();

        var response = await _client.PostAsync($"/api/v1/User/{nonExistentUserId}/restore", null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        content.Should().Contain("User not found");
    }

    [Fact(DisplayName = "USER_025 - Lấy thông tin người dùng hiện tại - Integration Test")]
    public async Task GetCurrentUser_IntegrationTest_ReturnsUserInfo()
    {
        var (user, token) = await CreateAndAuthenticateUserAsync(
            "user025",
            "user025@test.com",
            "Pass123!",
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<UserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Id.Should().Be(user!.Id);
        content.UserName.Should().Be("user025");
        content.Email.Should().Be("user025@test.com");
        content.FullName.Should().Be("Test user025");
        content.Gender.Should().Be(GenderStatus.Male);
        content.PhoneNumber.Should().Be("0123456789");
    }

    [Fact(DisplayName = "USER_026 - Lấy thông tin người dùng khi JWT không có trong header")]
    public async Task GetCurrentUser_NoJWT_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_027 - Cập nhật thông tin người dùng - Integration Test")]
    public async Task UpdateCurrentUser_IntegrationTest_UpdatesSuccessfully()
    {
        var (user, token) = await CreateAndAuthenticateUserAsync(
            "user027",
            "user027@test.com",
            "Pass123!",
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateUserCommand
        {
            FullName = "Updated Name",
            Gender = GenderStatus.Female,
            PhoneNumber = "0999888777"
        };

        var response = await _client.PutAsJsonAsync("/api/v1/User/me", updateRequest).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<UserDTOForManagerResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.FullName.Should().Be("Updated Name");
        content.Gender.Should().Be(GenderStatus.Female);
        content.PhoneNumber.Should().Be("0999888777");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updatedUser = await db.Users.FindAsync(user!.Id).ConfigureAwait(true);
        updatedUser.Should().NotBeNull();
        updatedUser!.FullName.Should().Be("Updated Name");
        updatedUser.Gender.Should().Be(GenderStatus.Female);
        updatedUser.PhoneNumber.Should().Be("0999888777");
    }

    [Fact(DisplayName = "USER_028 - Cập nhật thông tin với validation error - số điện thoại không hợp lệ")]
    public async Task UpdateCurrentUser_InvalidPhoneNumber_ReturnsBadRequest()
    {
        var (_, token) = await CreateAndAuthenticateUserAsync(
            "user028",
            "user028@test.com",
            "Pass123!",
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateUserCommand { PhoneNumber = "invalid-phone" };

        var response = await _client.PutAsJsonAsync("/api/v1/User/me", updateRequest).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        content.Should().Contain("Invalid phone number format");
    }

    [Fact(DisplayName = "USER_029 - Đổi mật khẩu - Integration Test")]
    public async Task ChangePassword_IntegrationTest_PasswordChangedAndTokenInvalidated()
    {
        var (_, token) = await CreateAndAuthenticateUserAsync(
            "user029",
            "user029@test.com",
            "OldPass123!",
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var changePasswordRequest = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass456!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordRequest)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<ChangePasswordByUserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Message.Should().Be("Password changed successfully");

        var loginRequest = new LoginCommand { UsernameOrEmail = "user029", Password = "NewPass456!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginRequest).ConfigureAwait(true);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var oldTokenResponse = await _client.GetAsync("/api/v1/User/me", CancellationToken.None).ConfigureAwait(true);
        oldTokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_030 - Đổi mật khẩu với CurrentPassword sai - Integration Test")]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsUnauthorized()
    {
        var (_, token) = await CreateAndAuthenticateUserAsync(
            "user030",
            "user030@test.com",
            "OldPass123!",
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var changePasswordRequest = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPass456!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordRequest)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        content.Should().Contain("Current password is incorrect");
    }

    [Fact(DisplayName = "USER_031 - Xóa tài khoản - Integration Test")]
    public async Task DeleteAccount_IntegrationTest_AccountDeletedAndTokenInvalidated()
    {
        var (user, token) = await CreateAndAuthenticateUserAsync(
            "user031",
            "user031@test.com",
            "Pass123!",
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/v1/User/delete-account", null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<DeleteAccountByUserReponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Message.Should().Be("Account deleted successfully");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var deletedUser = await db.Users.FindAsync(user!.Id).ConfigureAwait(true);
        deletedUser.Should().NotBeNull();
        deletedUser!.DeletedAt.Should().NotBeNull();

        var tokenTestResponse = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        tokenTestResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_032 - Xóa tài khoản khi đã bị Ban - Integration Test")]
    public async Task DeleteAccount_BannedAccount_ReturnsForbidden()
    {
        var (_, token) = await CreateAndAuthenticateUserAsync(
            "user032",
            "user032@test.com",
            "Pass123!",
            UserStatus.Banned, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/v1/User/delete-account", null).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        content.Should().Contain("Cannot delete banned account");
    }

    [Fact(DisplayName = "USER_033 - Kiểm tra SecurityStamp invalidation sau khi đổi mật khẩu")]
    public async Task SecurityStampInvalidation_AfterPasswordChange_OldTokenInvalid()
    {
        var (_, token1) = await CreateAndAuthenticateUserAsync(
            "user033",
            "user033@test.com",
            "Pass123!",
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var testResponse1 = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        testResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

        var changePasswordRequest = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            CurrentPassword = "Pass123!",
            NewPassword = "NewPass456!"
        };
        var changeResponse = await _client.PostAsJsonAsync("/api/v1/User/change-password", changePasswordRequest)
            .ConfigureAwait(true);
        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var testResponse2 = await _client.GetAsync("/api/v1/User/me", CancellationToken.None).ConfigureAwait(true);

        testResponse2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "USER_034 - Kiểm tra middleware chặn request khi tài khoản bị xóa mềm")]
    public async Task Middleware_BlocksDeletedAccount_ReturnsUnauthorized()
    {
        var (user, token) = await CreateAndAuthenticateUserAsync(
            "user034",
            "user034@test.com",
            "Pass123!",
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var testResponse1 = await _client.GetAsync("/api/v1/User/me", CancellationToken.None).ConfigureAwait(true);
        testResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var userToDelete = await db.Users.FindAsync(user!.Id).ConfigureAwait(true);
            userToDelete!.DeletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var testResponse2 = await _client.GetAsync("/api/v1/User/me", CancellationToken.None).ConfigureAwait(true);

        testResponse2.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var content = await testResponse2.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        content.Should().Contain("Account has been deleted");
    }

    [Fact(DisplayName = "USER_035 - Kiểm tra middleware chặn request khi tài khoản bị Ban")]
    public async Task Middleware_BlocksBannedAccount_ReturnsUnauthorized()
    {
        var (user, token) = await CreateAndAuthenticateUserAsync(
            "user035",
            "user035@test.com",
            "Pass123!",
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var testResponse1 = await _client.GetAsync("/api/v1/User/me").ConfigureAwait(true);
        testResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var userToBan = await db.Users.FindAsync(user!.Id).ConfigureAwait(true);
            userToBan!.Status = UserStatus.Banned;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var testResponse2 = await _client.GetAsync("/api/v1/User/me", CancellationToken.None).ConfigureAwait(true);

        testResponse2.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var content = await testResponse2.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        content.Should().Contain("Account has been banned");
    }
#pragma warning restore CRR0035
}
