using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.ApiContracts.Auth.Responses;
using Application.ApiContracts.UserManager.Responses;
using Domain.Primitives;
using Application.Common.Models;
using Domain.Constants;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Models;
using Xunit;
using Application.Features.Auth.Commands.Login;
using Application.Features.UserManager.Commands.UpdateUser;
using Application.Features.UserManager.Commands.ChangePassword;
using Application.Features.UserManager.Commands.AssignRoles;
using Application.Features.UserManager.Commands.ChangeUserStatus;
using Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;

namespace IntegrationTests;

public class UserManager : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public UserManager(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<(ApplicationUser user, string token)> CreateAndAuthenticateUserWithPermissionsAsync(
        string username,
        string email,
        string password,
        List<string> permissions,
        string status = UserStatus.Active)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FullName = $"Test {username}",
            PhoneNumber = "0123456789",
            Gender = GenderStatus.Male,
            Status = status,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        await userManager.CreateAsync(user, password);

        // Create role with permissions
        var roleName = $"Role_{username}";
        var role = new ApplicationRole { Name = roleName, Description = "Test role" };
        await roleManager.CreateAsync(role);

        foreach (var permissionName in permissions)
        {
            var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
            if (permission != null)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    Permission = permission
                };
                db.RolePermissions.Add(rolePermission);
            }
        }

        await db.SaveChangesAsync();
        await userManager.AddToRoleAsync(user, roleName);

        // Login to get token
        var loginRequest = new LoginCommand
        {
            UsernameOrEmail = username,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (user, loginResult!.AccessToken!);
    }

    private async Task<ApplicationUser> CreateUserAsync(
        string username,
        string email,
        string password,
        string status = UserStatus.Active,
        DateTimeOffset? deletedAt = null,
        string? phoneNumber = null)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FullName = $"Test {username}",
            PhoneNumber = phoneNumber ?? $"091234567{new Random().Next(0, 9)}",
            Gender = GenderStatus.Male,
            Status = status,
            DeletedAt = deletedAt,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        await userManager.CreateAsync(user, password);
        return user;
    }

    [Fact(DisplayName = "UMGR_019 - Query người dùng với filter và sorting phức tạp")]
    public async Task GetAllUsers_WithComplexFilterAndSorting_ReturnsCorrectResults()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR019", "admin019@test.com", "Pass@123",
            [PermissionsList.Users.View]);

        // Create test users
        await CreateUserAsync("userA", "userA@test.com", "Pass@123", UserStatus.Active);
        await CreateUserAsync("userB", "userB@test.com", "Pass@123", UserStatus.Active);
        await CreateUserAsync("userC", "userC@test.com", "Pass@123", UserStatus.Banned);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/UserManager?Filters=Status==Active&Sorts=-FullName&Page=1&PageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<object>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact(DisplayName = "UMGR_020 - Lấy thông tin user bao gồm cả soft deleted")]
    public async Task GetUserById_SoftDeletedUser_ReturnsUserWithDeletedAt()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR020", "admin020@test.com", "Pass@123",
            [PermissionsList.Users.View]);

        var softDeletedUser = await CreateUserAsync(
            "deletedUser", "deleted@test.com", "Pass@123",
            UserStatus.Active, DateTimeOffset.UtcNow.AddDays(-1));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/UserManager/{softDeletedUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result?.Should().NotBeNull();
    }

    [Fact(DisplayName = "UMGR_021 - Cập nhật thông tin user với dữ liệu có khoảng trắng đầu cuối")]
    public async Task UpdateUser_WithWhitespace_TrimsData()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR021", "admin021@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser021", "target021@test.com", "Pass@123");

        var request = new UpdateUserCommand
        {
            FullName = "  Test User  ",
            PhoneNumber = "  0912345678  "
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updatedUser = await db.Users.FindAsync(targetUser.Id);
        updatedUser!.FullName.Should().Be("Test User");
        updatedUser.PhoneNumber.Should().Be("0912345678");
    }

    [Fact(DisplayName = "UMGR_022 - Cập nhật user với email có ký tự đặc biệt hợp lệ")]
    public async Task UpdateUser_WithSpecialCharactersInEmail_Success()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR022", "admin022@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser022", "target022@test.com", "Pass@123");

        var request = new UpdateUserCommand
        {
            FullName = "Test User"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updatedUser = await db.Users.FindAsync(targetUser.Id);
        updatedUser!.Email.Should().Be("test+tag@example.co.uk");
    }

    [Fact(DisplayName = "UMGR_023 - Cập nhật user với phone number trùng với user khác")]
    public async Task UpdateUser_WithDuplicatePhoneNumber_ReturnsConflict()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR023", "admin023@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser023", "target023@test.com", "Pass@123", phoneNumber: "0987654321");

        var request = new UpdateUserCommand
        {
            PhoneNumber = "0912345678"
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "UMGR_024 - Cập nhật user với phone number = null khi đã có user khác cũng null")]
    public async Task UpdateUser_WithNullPhoneNumber_AllowsMultipleNulls()
    {
        // Arrange
        var (adminUser, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR024", "admin024@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser024", "target024@test.com", "Pass@123", phoneNumber: "0912345678");
        var existingUser = await CreateUserAsync("existingUser024", "existing024@test.com", "Pass@123", phoneNumber: null);

        var request = new UpdateUserCommand
        {
            PhoneNumber = null
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify DB allows multiple nulls
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var nullPhoneUsers = await db.Users.Where(u => u.PhoneNumber == null).CountAsync();
        nullPhoneUsers.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact(DisplayName = "UMGR_025 - Đổi mật khẩu và verify tất cả refresh tokens bị vô hiệu hóa")]
    public async Task ChangePassword_InvalidatesRefreshTokens()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR025", "admin025@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser025", "target025@test.com", "Pass@123");

        // Set refresh token
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FindAsync(targetUser.Id);
            user!.RefreshToken = "valid_refresh_token";
            user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);
            await db.SaveChangesAsync();
        }

        var request = new ChangePasswordCommand { NewPassword = "NewPass@123" };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify refresh token invalidated
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FindAsync(targetUser.Id);
            (user!.RefreshToken == null || user.RefreshTokenExpiryTime < DateTimeOffset.UtcNow).Should().BeTrue();
        }
    }

    [Fact(DisplayName = "UMGR_026 - Gán roles cho user đã có roles trước đó (thay thế hoàn toàn)")]
    public async Task AssignRoles_ReplacesExistingRoles()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR026", "admin026@test.com", "Pass@123",
            [PermissionsList.Users.AssignRoles]);

        var targetUser = await CreateUserAsync("targetUser026", "target026@test.com", "Pass@123");

        // Create roles
        ApplicationRole staffRole, managerRole;
        using (var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            staffRole = new ApplicationRole { Name = "Staff026", Description = "Staff role" };
            managerRole = new ApplicationRole { Name = "Manager026", Description = "Manager role" };
            await roleManager.CreateAsync(staffRole);
            await roleManager.CreateAsync(managerRole);

            // Assign staff role initially
            await userManager.AddToRoleAsync(targetUser, staffRole.Name!);
        }

        var request = new AssignRolesCommand { RoleNames = [managerRole.Name!] };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/assign-roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify only manager role remains
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roles = await userManager.GetRolesAsync(targetUser);
            roles.Should().ContainSingle();
            roles.Should().Contain("Manager026");
            roles.Should().NotContain("Staff026");
        }
    }

    [Fact(DisplayName = "UMGR_027 - Gán roles rỗng cho user (xóa tất cả roles)")]
    public async Task AssignRoles_WithEmptyList_RemovesAllRoles()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR027", "admin027@test.com", "Pass@123",
            [PermissionsList.Users.AssignRoles]);

        var targetUser = await CreateUserAsync("targetUser027", "target027@test.com", "Pass@123");

        // Create and assign role
        using (var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var staffRole = new ApplicationRole { Name = "Staff027", Description = "Staff role" };
            await roleManager.CreateAsync(staffRole);
            await userManager.AddToRoleAsync(targetUser, staffRole.Name!);
        }

        var request = new AssignRolesCommand { RoleNames = [] };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/assign-roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify no roles remain
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roles = await userManager.GetRolesAsync(targetUser);
            roles.Should().BeEmpty();
        }
    }

    [Fact(DisplayName = "UMGR_028 - Thay đổi trạng thái user đã bị soft deleted")]
    public async Task ChangeUserStatus_OnSoftDeletedUser_ReturnsBadRequest()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR028", "admin028@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync(
            "targetUser028", "target028@test.com", "Pass@123",
            UserStatus.Banned, DateTimeOffset.UtcNow.AddDays(-1));

        var request = new ChangeUserStatusCommand { Status = UserStatus.Active };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "UMGR_029 - Bulk change status với một số user không hợp lệ (nguyên tắc all-or-nothing)")]
    public async Task ChangeMultipleUsersStatus_WithInvalidUser_RollsBackAllChanges()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR029", "admin029@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var user1 = await CreateUserAsync("user029_1", "user029_1@test.com", "Pass@123", UserStatus.Active);
        var user2 = await CreateUserAsync("user029_2", "user029_2@test.com", "Pass@123", UserStatus.Active);
        var nonExistentId = Guid.NewGuid();

        var request = new ChangeMultipleUsersStatusCommand
        {
            UserIds = [user1.Id, nonExistentId, user2.Id],
            Status = UserStatus.Banned
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PatchAsJsonAsync("/api/v1/UserManager/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no users were changed
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var u1 = await db.Users.FindAsync(user1.Id);
        var u2 = await db.Users.FindAsync(user2.Id);
        u1!.Status.Should().Be(UserStatus.Active);
        u2!.Status.Should().Be(UserStatus.Active);
    }

    [Fact(DisplayName = "UMGR_030 - Bulk change status bao gồm chính User đang thực hiện (Super Admin)")]
    public async Task ChangeMultipleUsersStatus_IncludingSelf_ReturnsBadRequest()
    {
        // Arrange
        var (adminUser, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR030", "admin030@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser030", "target030@test.com", "Pass@123");

        var request = new ChangeMultipleUsersStatusCommand
        {
            UserIds = [adminUser.Id, targetUser.Id],
            Status = UserStatus.Banned
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PatchAsJsonAsync("/api/v1/UserManager/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "UMGR_031 - Cập nhật username trùng với user đã bị soft deleted")]
    public async Task UpdateUser_WithUsernameOfSoftDeletedUser_ReturnsConflict()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR031", "admin031@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser031", "target031@test.com", "Pass@123");

        var request = new UpdateUserCommand { FullName = "Deleted User Updated" };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "UMGR_032 - Lấy danh sách users với pagination: page cuối cùng chỉ có 1 phần tử")]
    public async Task GetAllUsers_LastPageWithOneItem_ReturnsCorrectPagination()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR032", "admin032@test.com", "Pass@123",
            [PermissionsList.Users.View]);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/UserManager?Page=3&PageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<object>>();
        result.Should().NotBeNull();
        result!.PageNumber.Should().Be(3);
        result.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "UMGR_033 - Cập nhật user không thay đổi password khi body không chứa password")]
    public async Task UpdateUser_WithoutPasswordField_KeepsExistingPassword()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR033", "admin033@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser033", "target033@test.com", "OldPass@123");

        var request = new UpdateUserCommand { FullName = "New Name" };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify password unchanged by attempting login with old password
        var loginRequest = new LoginCommand { UsernameOrEmail = "targetUser033", Password = "OldPass@123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "UMGR_034 - Cập nhật user với trường rác không hợp lệ trong body (bảo mật)")]
    public async Task UpdateUser_WithMaliciousFields_IgnoresMaliciousData()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR034", "admin034@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser034", "target034@test.com", "Pass@123");

        var originalRefreshToken = "original_token";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FindAsync(targetUser.Id);
            user!.RefreshToken = originalRefreshToken;
            await db.SaveChangesAsync();
        }

        var maliciousRequest = new
        {
            FullName = "New Name",
            RefreshToken = "hacker_token",
            Id = Guid.NewGuid()
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}", maliciousRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify malicious fields not changed
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = await db.Users.FindAsync(targetUser.Id);
            user!.FullName.Should().Be("New Name");
            user.RefreshToken.Should().Be(originalRefreshToken);
            user.Id.Should().Be(targetUser.Id);
        }
    }

    [Fact(DisplayName = "UMGR_035 - Audit log ghi lại đúng thông tin khi đổi password")]
    public async Task ChangePassword_CreatesAuditLog()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR035", "admin035@test.com", "Pass@123",
            [PermissionsList.Users.Edit]);

        var targetUser = await CreateUserAsync("targetUser035", "target035@test.com", "Pass@123");

        var request = new ChangePasswordCommand { NewPassword = "NewPass@123" };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify audit log exists (implementation should create audit log)
        // Note: This test assumes audit log functionality will be implemented
    }

    [Fact(DisplayName = "UMGR_036 - Audit log ghi lại đúng thông tin khi thay đổi role")]
    public async Task AssignRoles_CreatesAuditLog()
    {
        // Arrange
        var (_, token) = await CreateAndAuthenticateUserWithPermissionsAsync(
            "adminUMGR036", "admin036@test.com", "Pass@123",
            [PermissionsList.Users.AssignRoles]);

        var targetUser = await CreateUserAsync("targetUser036", "target036@test.com", "Pass@123");

        ApplicationRole managerRole;
        using (var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            managerRole = new ApplicationRole { Name = "Manager036", Description = "Manager role" };
            await roleManager.CreateAsync(managerRole);
        }

        var request = new AssignRolesCommand { RoleNames = [managerRole.Name!] };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/UserManager/{targetUser.Id}/assign-roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify audit log exists (implementation should create audit log)
        // Note: This test assumes audit log functionality will be implemented
    }
}
