using System.Net;
using System.Net.Http.Json;
using Application.ApiContracts.Permission.Responses;
using Application.Features.Permissions.Commands.CreateRole;
using Application.Features.Permissions.Commands.UpdateRolePermissions;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using PermissionEntity = Domain.Entities.Permission;

namespace IntegrationTests;

public class PermissionAndRole : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public PermissionAndRole(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact(DisplayName = "PERM_INT_001 - API lấy tất cả permissions trả về đầy đủ thông tin")]
    public async Task GetAllPermissions_ReturnsFullPermissionList()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/Permission/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, List<PermissionResponse>>>();
        content.Should().NotBeNull();
        content.Should().ContainKey("Brands");
        content.Should().ContainKey("Products");
        content.Should().ContainKey("Roles");
        content!["Brands"].Should().NotBeEmpty();
        content["Brands"].Should().AllSatisfy(p =>
        {
            p.ID.Should().NotBeNullOrEmpty();
            p.DisplayName.Should().NotBeNullOrEmpty();
            p.Description.Should().NotBeNullOrEmpty();
        });
    }

    [Fact(DisplayName = "PERM_INT_002 - API lấy permissions của user hiện tại khi đã đăng nhập")]
    public async Task GetMyPermissions_AuthenticatedUser_ReturnsUserPermissions()
    {
        // Arrange
        var (user, _) = await CreateUserWithRoleAndPermissionsAsync(
            "editor_int002@test.com", 
            "Editor_INT002",
            [ PermissionsList.Brands.View, PermissionsList.Brands.Create, PermissionsList.Products.View, PermissionsList.Products.Create, PermissionsList.Files.View, PermissionsList.Files.Upload ]);

        await AuthenticateAsUserAsync(user.Email!);

        // Act
        var response = await _client.GetAsync("/api/v1/Permission/my-permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PermissionAndRoleOfUserResponse>();
        content.Should().NotBeNull();
        content!.UserId.Should().Be(user.Id);
        content.Permissions.Should().HaveCount(6);
    }

    [Fact(DisplayName = "PERM_INT_003 - API lấy permissions của user hiện tại khi chưa đăng nhập")]
    public async Task GetMyPermissions_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange - No authentication

        // Act
        var response = await _client.GetAsync("/api/v1/Permission/my-permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "PERM_INT_004 - API lấy permissions của user khác bằng userId")]
    public async Task GetUserPermissionsById_WithViewPermission_ReturnsTargetUserPermissions()
    {
        // Arrange
        var (adminUser, _) = await CreateUserWithRoleAndPermissionsAsync(
            "admin_int004@test.com",
            "Admin_INT004",
            [PermissionsList.Roles.View]);

        var (targetUser, _) = await CreateUserWithRoleAndPermissionsAsync(
            "staff_int004@test.com",
            "Staff_INT004",
            [ PermissionsList.Products.View, PermissionsList.Brands.View, PermissionsList.Files.View]);

        await AuthenticateAsUserAsync(adminUser.Email!);

        // Act
        var response = await _client.GetAsync($"/api/v1/Permission/users/{targetUser.Id}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PermissionAndRoleOfUserResponse>();
        content.Should().NotBeNull();
        content!.UserId.Should().Be(targetUser.Id);
        content.Permissions.Should().HaveCount(3);
        content.Email.Should().Be("staff_int004@test.com");
    }

    [Fact(DisplayName = "PERM_INT_005 - API lấy permissions của user khác khi không có quyền")]
    public async Task GetUserPermissionsById_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var (normalUser, _) = await CreateUserWithRoleAndPermissionsAsync(
            "user_int005@test.com",
            "User_INT005",
            [PermissionsList.Brands.View]);

        var (targetUser, _) = await CreateUserWithRoleAndPermissionsAsync(
            "target_int005@test.com",
            "Target_INT005",
            [PermissionsList.Products.View]);

        await AuthenticateAsUserAsync(normalUser.Email!);

        // Act
        var response = await _client.GetAsync($"/api/v1/Permission/users/{targetUser.Id}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PERM_INT_006 - API lấy permissions của role hợp lệ")]
    public async Task GetRolePermissions_ValidRole_ReturnsRolePermissions()
    {
        // Arrange
        var (adminUser, _) = await CreateUserWithRoleAndPermissionsAsync(
            "admin_int006@test.com",
            "Admin_INT006",
            [PermissionsList.Roles.View]);

        var testRole = await CreateRoleWithPermissionsAsync(
            "Manager_INT006",
            [ PermissionsList.Brands.View, PermissionsList.Brands.Create, PermissionsList.Brands.Edit, 
                    PermissionsList.Brands.Delete, PermissionsList.Products.View ]);

        await AuthenticateAsUserAsync(adminUser.Email!);

        // Act
        var response = await _client.GetAsync($"/api/v1/Permission/roles/{testRole.Name}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<PermissionResponse>>();
        content.Should().NotBeNull();
        content.Should().HaveCount(5);
    }

    [Fact(DisplayName = "PERM_INT_007 - API tạo role mới thành công")]
    public async Task CreateRole_ValidData_CreatesRoleSuccessfully()
    {
        // Arrange
        var (adminUser, adminRole) = await CreateUserWithRoleAndPermissionsAsync(
            "admin_int007@test.com",
            "Admin_INT007",
            [PermissionsList.Roles.Create]);

        await AuthenticateAsUserAsync(adminUser.Email!);

        var request = new CreateRoleCommand
        {
            RoleName = "NewRole_INT007",
            Description = "Integration Test Role",
            Permissions = [PermissionsList.Brands.View, PermissionsList.Products.View]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<RoleCreateResponse>();
        content.Should().NotBeNull();
        content!.RoleId.Should().NotBeEmpty();
        content.RoleName.Should().Be("NewRole_INT007");
        content.Description.Should().Be("Integration Test Role");
        content.Permissions.Should().HaveCount(2);

        // Verify in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles.FirstOrDefaultAsync(r => r.Name == "NewRole_INT007");
        roleInDb.Should().NotBeNull();
        roleInDb!.Description.Should().Be("Integration Test Role");
    }

    [Fact(DisplayName = "PERM_INT_008 - API tạo role mới khi không có quyền")]
    public async Task CreateRole_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var (normalUser, normalRole) = await CreateUserWithRoleAndPermissionsAsync(
            "user_int008@test.com",
            "User_INT008",
            [PermissionsList.Brands.View]);

        await AuthenticateAsUserAsync(normalUser.Email!);

        var request = new CreateRoleCommand
        {
            RoleName = "Unauthorized_INT008",
            Description = "Should fail",
            Permissions = [PermissionsList.Brands.View]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Verify role not created in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Unauthorized_INT008");
        roleInDb.Should().BeNull();
    }

    [Fact(DisplayName = "PERM_INT_009 - API tạo role với tên trùng lặp")]
    public async Task CreateRole_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var (adminUser, _) = await CreateUserWithRoleAndPermissionsAsync(
            "admin_int009@test.com",
            "Admin_INT009",
            [PermissionsList.Roles.Create]);

        await AuthenticateAsUserAsync(adminUser.Email!);

        var request = new CreateRoleCommand
        {
            RoleName = "DuplicateRole_INT009",
            Description = "Duplicate attempt",
            Permissions = [PermissionsList.Products.View]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("already exists");
    }

    [Fact(DisplayName = "PERM_INT_010 - API cập nhật permissions của role thành công")]
    public async Task UpdateRolePermissions_ValidData_UpdatesSuccessfully()
    {
        // Arrange
        var (adminUser, adminRole) = await CreateUserWithRoleAndPermissionsAsync(
            "admin_int010@test.com",
            "Admin_INT010",
            [PermissionsList.Roles.AssignPermissions]);

        var testRole = await CreateRoleWithPermissionsAsync(
            "TestRole_INT010",
            [PermissionsList.Brands.View, PermissionsList.Brands.Create]);

        await AuthenticateAsUserAsync(adminUser.Email!);

        var request = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommand
        {
            Permissions = [PermissionsList.Products.View, PermissionsList.Products.Create, PermissionsList.Products.Edit]
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/Permission/roles/{testRole.Name}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Name == "TestRole_INT010");
        roleInDb.Should().NotBeNull();
        roleInDb!.RolePermissions.Should().HaveCount(3);
    }

    [Fact(DisplayName = "PERM_INT_011 - API cập nhật role (description) thành công")]
    public async Task UpdateRole_UpdateDescription_UpdatesSuccessfully()
    {
        // Arrange
        var (adminUser, adminRole) = await CreateUserWithRoleAndPermissionsAsync(
            "admin_int011@test.com",
            "Admin_INT011",
            [PermissionsList.Roles.Edit]);

        var testRole = await CreateRoleWithPermissionsAsync(
            "TestRole_INT011",
            [PermissionsList.Brands.View],
            "Original Description");

        await AuthenticateAsUserAsync(adminUser.Email!);

        var request = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommand
        {
            Description = "Updated Description"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/Permission/roles/{testRole.Name}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles.FirstOrDefaultAsync(r => r.Name == "TestRole_INT011");
        roleInDb.Should().NotBeNull();
        roleInDb!.Description.Should().Be("Updated Description");
    }

    [Fact(DisplayName = "PERM_INT_012 - API xóa role thành công")]
    public async Task DeleteRole_ValidRole_DeletesSuccessfully()
    {
        // Arrange
        var (adminUser, adminRole) = await CreateUserWithRoleAndPermissionsAsync(
            "admin_int012@test.com",
            "Admin_INT012",
            [PermissionsList.Roles.Delete]);

        var testRole = await CreateRoleWithPermissionsAsync(
            "TestRole_INT012",
            [PermissionsList.Brands.View]);

        await AuthenticateAsUserAsync(adminUser.Email!);

        // Act
        var response = await _client.DeleteAsync($"/api/v1/Permission/roles/{testRole.Name}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify removed from DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles.FirstOrDefaultAsync(r => r.Name == "TestRole_INT012");
        roleInDb.Should().BeNull();
    }

    [Fact(DisplayName = "PERM_INT_013 - API xóa role khi không có quyền")]
    public async Task DeleteRole_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var (normalUser, normalRole) = await CreateUserWithRoleAndPermissionsAsync(
            "user_int013@test.com",
            "User_INT013",
            [PermissionsList.Brands.View]);

        var testRole = await CreateRoleWithPermissionsAsync(
            "TestRole_INT013",
            [PermissionsList.Brands.View]);

        await AuthenticateAsUserAsync(normalUser.Email!);

        // Act
        var response = await _client.DeleteAsync($"/api/v1/Permission/roles/{testRole.Name}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Verify still exists in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles.FirstOrDefaultAsync(r => r.Name == "TestRole_INT013");
        roleInDb.Should().NotBeNull();
    }

    [Fact(DisplayName = "PERM_INT_014 - API xóa nhiều roles thành công")]
    public async Task DeleteMultipleRoles_ValidRoles_DeletesAllSuccessfully()
    {
        // Arrange
        var (adminUser, adminRole) = await CreateUserWithRoleAndPermissionsAsync(
            "admin_int014@test.com",
            "Admin_INT014",
            [PermissionsList.Roles.Delete]);

        var role1 = await CreateRoleWithPermissionsAsync("Role1_INT014", [PermissionsList.Brands.View]);
        var role2 = await CreateRoleWithPermissionsAsync("Role2_INT014", [PermissionsList.Products.View]);
        var role3 = await CreateRoleWithPermissionsAsync("Role3_INT014", [PermissionsList.Files.View]);

        await AuthenticateAsUserAsync(adminUser.Email!);

        var request = new List<string> { "Role1_INT014", "Role2_INT014", "Role3_INT014" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles/delete-multiple", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<RoleDeleteResponse>();
        content.Should().NotBeNull();
        content!.Message.Should().Contain("3");

        // Verify all removed from DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var rolesInDb = await db.Roles.Where(r => r.Name!.Contains("INT014")).ToListAsync();
        rolesInDb.Should().HaveCount(1); // Only Admin_INT014 should remain
    }

    [Fact(DisplayName = "PERM_INT_015 - API lấy tất cả roles")]
    public async Task GetAllRoles_WithViewPermission_ReturnsAllRoles()
    {
        // Arrange
        var (adminUser, adminRole) = await CreateUserWithRoleAndPermissionsAsync(
            "admin_int015@test.com",
            "Admin_INT015",
            [PermissionsList.Roles.View]);

        await CreateRoleWithPermissionsAsync("Role1_INT015", [PermissionsList.Brands.View]);
        await CreateRoleWithPermissionsAsync("Role2_INT015", [ PermissionsList.Products.View ]);
        await CreateRoleWithPermissionsAsync("Role3_INT015", [ PermissionsList.Files.View ]);
        await CreateRoleWithPermissionsAsync("Role4_INT015", [ PermissionsList.Suppliers.View ]);
        await CreateRoleWithPermissionsAsync("Role5_INT015", [ PermissionsList.Inputs.View ]);

        await AuthenticateAsUserAsync(adminUser.Email!);

        // Act
        var response = await _client.GetAsync("/api/v1/Permission/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<RoleSelectResponse>>();
        content.Should().NotBeNull();
        content.Should().HaveCountGreaterThanOrEqualTo(5);
        content.Should().Contain(r => r.Name == "Role1_INT015");
        content.Should().Contain(r => r.Name == "Role5_INT015");
    }

    // Helper Methods

    private async Task<(ApplicationUser user, ApplicationRole role)> CreateUserWithRoleAndPermissionsAsync(
        string email, string roleName, string[] permissionNames)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var user = new ApplicationUser
        {
            UserName = email.Split('@')[0],
            Email = email,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user, "Password123!");

        var role = new ApplicationRole
        {
            Name = roleName,
            Description = $"Test role {roleName}"
        };

        await roleManager.CreateAsync(role);
        await userManager.AddToRoleAsync(user, roleName);

        var permissions = await db.Permissions.Where(p => permissionNames.Contains(p.Name)).ToListAsync();
        foreach (var permission in permissions)
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }

        await db.SaveChangesAsync();

        return (user, role);
    }

    private async Task<ApplicationRole> CreateRoleWithPermissionsAsync(
        string roleName, string[] permissionNames, string? description = null)
    {
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var role = new ApplicationRole
        {
            Name = roleName,
            Description = description ?? $"Test role {roleName}"
        };

        await roleManager.CreateAsync(role);

        var permissions = await db.Permissions.Where(p => permissionNames.Contains(p.Name)).ToListAsync();
        foreach (var permission in permissions)
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }

        await db.SaveChangesAsync();

        return role;
    }

    private async Task AuthenticateAsAdminAsync()
    {
        // Implementation depends on your authentication mechanism
        // This is a placeholder - adjust based on your IntegrationTestWebAppFactory setup
        throw new NotImplementedException();
    }

    private async Task AuthenticateAsUserAsync(string email)
    {
        // Implementation depends on your authentication mechanism
        // This is a placeholder - adjust based on your IntegrationTestWebAppFactory setup
        throw new NotImplementedException();
    }
}
