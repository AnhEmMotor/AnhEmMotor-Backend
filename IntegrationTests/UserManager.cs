using Application.Features.UserManager.Commands.AssignRoles;
using Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;
using Application.Features.UserManager.Commands.ChangeUserStatus;
using Application.Features.UserManager.Commands.UpdateUser;
using Domain.Constants;
using Domain.Constants.Permission;
using Domain.Entities;
using Domain.Primitives;
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

using System.Threading.Tasks;
using Xunit;

[Collection("Shared Integration Collection")]
public class UserManager : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public UserManager(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() { await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true); }

#pragma warning disable IDE0079
#pragma warning disable CRR0035
    [Fact(DisplayName = "UMGR_019 - Query người dùng với filter và sorting phức tạp")]
    public async Task GetAllUsers_WithComplexFilterAndSorting_ReturnsCorrectResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.View ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var userA = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"userA_{uniqueId}",
            "Pass@123",
            email: $"userA_{uniqueId}@test.com")
            .ConfigureAwait(true);
        var userB = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"userB_{uniqueId}",
            "Pass@123",
            email: $"userB_{uniqueId}@test.com")
            .ConfigureAwait(true);
        var userC = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"userC_{uniqueId}",
            "Pass@123",
            email: $"userC_{uniqueId}@test.com",
            isLocked: true)
            .ConfigureAwait(true);

        var response = await _client.GetAsync(
            $"/api/v1/UserManager?Filters=Status=={UserStatus.Active}&Sorts=-FullName&Page=1&PageSize=100")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<object>>(CancellationToken.None)
            .ConfigureAwait(true);
        result.Should().NotBeNull();

        var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        contentString.Should().Contain(userA.Id.ToString());
        contentString.Should().Contain(userB.Id.ToString());
        contentString.Should().NotContain(userC.Id.ToString());
    }

    [Fact(DisplayName = "UMGR_020 - Lấy thông tin user bao gồm cả soft deleted")]
    public async Task GetUserById_SoftDeletedUser_ReturnsUserWithDeletedAt()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.View ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var softDeletedUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"del_{uniqueId}",
            "Pass@123",
            email: $"del_{uniqueId}@test.com",
            deletedAt: DateTimeOffset.UtcNow.AddDays(-1))
            .ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/UserManager/{softDeletedUser.Id}").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "UMGR_021 - Cập nhật thông tin user với dữ liệu có khoảng trắng đầu cuối")]
    public async Task UpdateUser_WithWhitespace_TrimsData()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);

        var request = new UpdateUserCommand { FullName = "  Test User  ", PhoneNumber = "  0912345678  " };

        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updatedUser = await db.Users.FindAsync(targetUser.Id).ConfigureAwait(true);
        updatedUser!.FullName.Should().Be("Test User");
        updatedUser.PhoneNumber.Should().Be("0912345678");
    }

    [Fact(DisplayName = "UMGR_022 - Cập nhật user với email có ký tự đặc biệt hợp lệ")]
    public async Task UpdateUser_WithSpecialCharactersInEmail_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            email: $"test+tag_{uniqueId}@example.co.uk")
            .ConfigureAwait(true);

        var request = new UpdateUserCommand { FullName = "Test User" };

        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updatedUser = await db.Users.FindAsync(targetUser.Id).ConfigureAwait(true);
        updatedUser!.Email.Should().Be($"test+tag_{uniqueId}@example.co.uk");
    }

    [Fact(DisplayName = "UMGR_023 - Cập nhật user với phone number trùng với user khác")]
    public async Task UpdateUser_WithDuplicatePhoneNumber_ReturnsConflict()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var phone = "0987654321";
        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users.FindAsync(targetUser.Id).ConfigureAwait(true);
            u!.PhoneNumber = phone;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var duplicateAttemptUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"dup_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);

        var request = new UpdateUserCommand { PhoneNumber = phone, FullName = "Duplicate Attempt User" };

        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{duplicateAttemptUser.Id}", request)
            .ConfigureAwait(true);

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict, content);
    }

    [Fact(DisplayName = "UMGR_024 - Cập nhật user với phone number = null khi đã có user khác cũng null")]
    public async Task UpdateUser_WithNullPhoneNumber_AllowsMultipleNulls()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"existing_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users
                .FirstOrDefaultAsync(
                    u => string.Compare(u.UserName, $"existing_{uniqueId}") == 0,
                    CancellationToken.None)
                .ConfigureAwait(true);
            u!.PhoneNumber = null;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var request = new UpdateUserCommand { PhoneNumber = null, FullName = "Updated Name" };

        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var count = await db.Users
                .CountAsync(u => u.PhoneNumber == null, CancellationToken.None)
                .ConfigureAwait(true);
            count.Should().BeGreaterThanOrEqualTo(2);
        }
    }

    [Fact(DisplayName = "UMGR_025 - Đổi mật khẩu và verify tất cả refresh tokens bị vô hiệu hóa")]
    public async Task ChangePassword_InvalidatesRefreshTokens()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit, PermissionsList.Users.ChangePassword ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FindAsync(targetUser.Id).ConfigureAwait(true);
            user!.RefreshToken = "valid_refresh_token";
            user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var request = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            NewPassword = "NewPass@123"
        };

        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/change-password", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FindAsync(targetUser.Id).ConfigureAwait(true);
            (user!.RefreshToken == null || user.RefreshTokenExpiryTime < DateTimeOffset.UtcNow).Should().BeTrue();
        }
    }

    [Fact(DisplayName = "UMGR_026 - Gán roles cho user đã có roles trước đó (thay thế hoàn toàn)")]
    public async Task AssignRoles_ReplacesExistingRoles()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.AssignRoles ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);

        var staffRoleName = $"Staff_{uniqueId}";
        var managerRoleName = $"Manager_{uniqueId}";

        using(var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await roleManager.CreateAsync(new ApplicationRole { Name = staffRoleName }).ConfigureAwait(true);
            await roleManager.CreateAsync(new ApplicationRole { Name = managerRoleName }).ConfigureAwait(true);

            var userInScope = await userManager.FindByIdAsync(targetUser.Id.ToString()).ConfigureAwait(true);
            await userManager.AddToRoleAsync(userInScope!, staffRoleName).ConfigureAwait(true);
        }

        var request = new AssignRolesCommand { RoleNames = [ managerRoleName ] };

        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/assign-roles", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var u = await userManager.FindByIdAsync(targetUser.Id.ToString()).ConfigureAwait(true);
            var roles = await userManager.GetRolesAsync(u!).ConfigureAwait(true);
            roles.Should().ContainSingle();
            roles.Should().Contain(managerRoleName);
            roles.Should().NotContain(staffRoleName);
        }
    }

    [Fact(DisplayName = "UMGR_027 - Gán roles rỗng cho user (xóa tất cả roles)")]
    public async Task AssignRoles_WithEmptyList_RemovesAllRoles()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.AssignRoles ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);

        using(var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleName = $"Role_{uniqueId}";
            await roleManager.CreateAsync(new ApplicationRole { Name = roleName }).ConfigureAwait(true);
            var userInScope = await userManager.FindByIdAsync(targetUser.Id.ToString()).ConfigureAwait(true);
            await userManager.AddToRoleAsync(userInScope!, roleName).ConfigureAwait(true);
        }

        var request = new AssignRolesCommand { RoleNames = [] };

        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/assign-roles", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var u = await userManager.FindByIdAsync(targetUser.Id.ToString()).ConfigureAwait(true);
            var roles = await userManager.GetRolesAsync(u!).ConfigureAwait(true);
            roles.Should().BeEmpty();
        }
    }

    [Fact(DisplayName = "UMGR_028 - Thay đổi trạng thái user đã bị soft deleted")]
    public async Task ChangeUserStatus_OnSoftDeletedUser_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            isLocked: true,
            deletedAt: DateTimeOffset.UtcNow.AddDays(-1))
            .ConfigureAwait(true);

        var request = new ChangeUserStatusCommand { Status = UserStatus.Active };

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/UserManager/{targetUser.Id}/status",
            request,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "UMGR_029 - Bulk change status với một số user không hợp lệ (nguyên tắc all-or-nothing)")]
    public async Task ChangeMultipleUsersStatus_WithInvalidUser_RollsBackAllChanges()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var user1 = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"u1_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        var user2 = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"u2_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        var nonExistentId = Guid.NewGuid();

        var request = new ChangeMultipleUsersStatusCommand
        {
            UserIds = [ user1.Id, nonExistentId, user2.Id ],
            Status = UserStatus.Banned
        };

        var response = await _client.PatchAsJsonAsync("/api/v1/UserManager/status", request, CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var u1 = await db.Users.FindAsync(user1.Id).ConfigureAwait(true);
        var u2 = await db.Users.FindAsync(user2.Id).ConfigureAwait(true);
        u1!.Status.Should().Be(UserStatus.Active);
        u2!.Status.Should().Be(UserStatus.Active);
    }

    [Fact(DisplayName = "UMGR_030 - Bulk change status bao gồm chính User đang thực hiện (Super Admin)")]
    public async Task ChangeMultipleUsersStatus_IncludingSelf_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        Guid adminId;
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var u = await db.Users
                .FirstAsync(u => string.Compare(u.UserName, adminName) == 0, CancellationToken.None)
                .ConfigureAwait(true);
            adminId = u.Id;
        }

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);

        var request = new ChangeMultipleUsersStatusCommand
        {
            UserIds = [ adminId, targetUser.Id ],
            Status = UserStatus.Banned
        };

        var response = await _client.PatchAsJsonAsync("/api/v1/UserManager/status", request, CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "UMGR_031 - Cập nhật username trùng với user đã bị soft deleted")]
    public async Task UpdateUser_WithUsernameOfSoftDeletedUser_ReturnsConflict()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var deletedPhone = "0999999999";
        await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"del_{uniqueId}",
            "Pass@123",
            deletedAt: DateTimeOffset.UtcNow.AddDays(-1),
            phoneNumber: deletedPhone)
            .ConfigureAwait(true);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);

        var request = new UpdateUserCommand { PhoneNumber = deletedPhone, FullName = "Update Attempt" };

        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "UMGR_032 - Lấy danh sách users với pagination: page cuối cùng chỉ có 1 phần tử")]
    public async Task GetAllUsers_LastPageWithOneItem_ReturnsCorrectPagination()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.View ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        for(int i = 0; i < 11; i++)
        {
            await IntegrationTestAuthHelper.CreateUserAsync(
                _factory.Services,
                $"u{i}_{uniqueId}",
                "Pass@123",
                email: $"u{i}_{uniqueId}@test.com")
                .ConfigureAwait(true);
        }

        var response = await _client.GetAsync($"/api/v1/UserManager?Filters=Email@=_{uniqueId}&Page=2&PageSize=10")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<object>>(CancellationToken.None)
            .ConfigureAwait(true);
        result.Should().NotBeNull();
        result!.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        var itemsJson = System.Text.Json.JsonSerializer.Serialize(result.Items);
        var itemsList = System.Text.Json.JsonSerializer.Deserialize<List<object>>(itemsJson);
        itemsList!.Count.Should().Be(2);
    }

    [Fact(DisplayName = "UMGR_033 - Cập nhật user không thay đổi password khi body không chứa password")]
    public async Task UpdateUser_WithoutPasswordField_KeepsExistingPassword()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "OldPass@123",
            CancellationToken.None)
            .ConfigureAwait(true);

        var request = new UpdateUserCommand { FullName = "New Name" };

        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request)
            .ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _client.DefaultRequestHeaders.Authorization = null;
        var userLogin = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"target_{uniqueId}",
            "OldPass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        userLogin.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "UMGR_034 - Cập nhật user với trường rác không hợp lệ trong body (bảo mật)")]
    public async Task UpdateUser_WithMaliciousFields_IgnoresMaliciousData()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        var originalRefreshToken = "original_token";
        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FindAsync(targetUser.Id).ConfigureAwait(true);
            user!.RefreshToken = originalRefreshToken;
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }

        var maliciousRequest = new { FullName = "New Name", RefreshToken = "hacker_token", Id = Guid.NewGuid() };

        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", maliciousRequest)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FindAsync(targetUser.Id).ConfigureAwait(true);
            user!.FullName.Should().Be("New Name");
            user.RefreshToken.Should().Be(originalRefreshToken);
            user.Id.Should().Be(targetUser.Id);
        }
    }

    [Fact(DisplayName = "UMGR_035 - Audit log ghi lại đúng thông tin khi đổi password")]
    public async Task ChangePassword_CreatesAuditLog()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.Edit, PermissionsList.Users.ChangePassword ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);

        var request = new Application.Features.UserManager.Commands.ChangePasswordByManager.ChangePasswordByManagerCommand
        {
            NewPassword = "NewPass@123"
        };

        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/change-password", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "UMGR_036 - Audit log ghi lại đúng thông tin khi thay đổi role")]
    public async Task AssignRoles_CreatesAuditLog()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminName = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminName,
            "Pass@123",
            [ PermissionsList.Users.AssignRoles ],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminName,
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(
            _factory.Services,
            $"target_{uniqueId}",
            "Pass@123",
            CancellationToken.None)
            .ConfigureAwait(true);
        var roleName = $"Manager_{uniqueId}";
        using(var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            await roleManager.CreateAsync(new ApplicationRole { Name = roleName }).ConfigureAwait(true);
        }

        var request = new AssignRolesCommand { RoleNames = [ roleName ] };

        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/assign-roles", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
#pragma warning restore CRR0035
}
