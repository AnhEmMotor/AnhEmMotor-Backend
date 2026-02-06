using Application.ApiContracts.Permission.Responses;
using Application.Features.Permissions.Commands.CreateRole;
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
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace IntegrationTests;

[Collection("Shared Integration Collection")]
public class PermissionAndRole : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public PermissionAndRole(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact(DisplayName = "PERM_INT_001 - API lấy tất cả permissions trả về đầy đủ thông tin")]
    public async Task GetAllPermissions_ReturnsFullPermissionList()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Roles.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Seed expected permissions
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var permissionsToSeed = new[] 
            { 
                PermissionsList.Brands.View, 
                PermissionsList.Products.View 
            };
            foreach (var permName in permissionsToSeed)
            {
                await EnsurePermissionExistsAsync(db, permName);
            }
        }

        var response = await _client.GetAsync("/api/v1/Permission/permissions", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<Dictionary<string, List<PermissionResponse>>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content.Should().ContainKey("Brands");
        content.Should().ContainKey("Products");
        content.Should().ContainKey("Roles");
        content!["Brands"].Should().NotBeEmpty();
        content["Brands"].Should()
            .AllSatisfy(
                p =>
                {
                    p.ID.Should().NotBeNullOrEmpty();
                    p.DisplayName.Should().NotBeNullOrEmpty();
                    p.Description.Should().NotBeNullOrEmpty();
                });
    }

    [Fact(DisplayName = "PERM_INT_002 - API lấy permissions của user hiện tại khi đã đăng nhập")]
    public async Task GetMyPermissions_AuthenticatedUser_ReturnsUserPermissions()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var permissions = new List<string> 
        { 
            PermissionsList.Brands.View, 
            PermissionsList.Brands.Create, 
            PermissionsList.Products.View, 
            PermissionsList.Products.Create, 
            PermissionsList.Files.View, 
            PermissionsList.Files.Upload 
        };
        
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, 
            username, 
            "Password123!", 
            permissions);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync("/api/v1/Permission/my-permissions", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PermissionAndRoleOfUserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.UserId.Should().Be(user.Id);
        content.Permissions.Should().HaveCount(6);
    }

    [Fact(DisplayName = "PERM_INT_003 - API lấy permissions của user hiện tại khi chưa đăng nhập")]
    public async Task GetMyPermissions_Unauthenticated_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/v1/Permission/my-permissions").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "PERM_INT_004 - API lấy permissions của user khác bằng userId")]
    public async Task GetUserPermissionsById_WithViewPermission_ReturnsTargetUserPermissions()
    {
        // 1. Create Target User
        var targetUniqueId = Guid.NewGuid().ToString("N")[..8];
        var targetUsername = $"target_{targetUniqueId}";
        var targetUser = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, 
            targetUsername, 
            "Password123!", 
            [PermissionsList.Products.View, PermissionsList.Brands.View, PermissionsList.Files.View]);

        // 2. Create Calling User
        var callerUniqueId = Guid.NewGuid().ToString("N")[..8];
        var callerUsername = $"caller_{callerUniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, 
            callerUsername, 
            "Password123!", 
            [PermissionsList.Users.View]);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, callerUsername, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync(
            $"/api/v1/Permission/users/{targetUser.Id}/permissions",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PermissionAndRoleOfUserResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.UserId.Should().Be(targetUser.Id);
        content.Permissions.Should().HaveCount(3);
    }

    [Fact(DisplayName = "PERM_INT_005 - API lấy permissions của user khác khi không có quyền")]
    public async Task GetUserPermissionsById_WithoutPermission_ReturnsForbidden()
    {
        // 1. Create Target User
        var targetUniqueId = Guid.NewGuid().ToString("N")[..8];
        var targetUsername = $"target_{targetUniqueId}";
        var targetUser = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, 
            targetUsername, 
            "Password123!", 
            [PermissionsList.Products.View]);

        // 2. Create Calling User WITHOUT Users.View
        var callerUniqueId = Guid.NewGuid().ToString("N")[..8];
        var callerUsername = $"caller_{callerUniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services, 
            callerUsername, 
            "Password123!", 
            [PermissionsList.Brands.View]);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, callerUsername, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync($"/api/v1/Permission/users/{targetUser.Id}/permissions")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PERM_INT_006 - API lấy permissions của role hợp lệ")]
    public async Task GetRolePermissions_ValidRole_ReturnsRolePermissions()
    {
        var roleName = $"Manager_{Guid.NewGuid():N}";
        await CreateRoleWithPermissionsInternalAsync(roleName, [PermissionsList.Brands.View, PermissionsList.Brands.Create, PermissionsList.Brands.Edit, PermissionsList.Brands.Delete, PermissionsList.Products.View]);

        // Caller
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"caller_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Roles.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync(
            $"/api/v1/Permission/roles/{roleName}/permissions",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<List<PermissionResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content.Should().HaveCount(5);
    }

    [Fact(DisplayName = "PERM_INT_007 - API tạo role mới thành công")]
    public async Task CreateRole_ValidData_CreatesRoleSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"admin_{uniqueId}";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Roles.Create]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await EnsurePermissionExistsAsync(db, PermissionsList.Brands.View);
            await EnsurePermissionExistsAsync(db, PermissionsList.Products.View);
        }

        var newRoleName = $"NewRole{uniqueId}";
        var request = new CreateRoleCommand
        {
            RoleName = newRoleName,
            Description = "Integration Test Role",
            Permissions = [PermissionsList.Brands.View, PermissionsList.Products.View]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/permission/roles", request).ConfigureAwait(true);

        var contentString = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, contentString);

        var content = await response.Content
            .ReadFromJsonAsync<RoleCreateResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        content.Should().NotBeNull();
        content!.RoleId.Should().NotBeEmpty();
        content.RoleName.Should().Be(newRoleName);
        content.Description.Should().Be("Integration Test Role");
        content.Permissions.Should().HaveCount(2);

        using (var verifyScope = _factory.Services.CreateScope())
        {
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var roleInDb = await verifyDb.Roles
                .FirstOrDefaultAsync(r => r.Name == newRoleName, CancellationToken.None)
                .ConfigureAwait(true);

            roleInDb.Should().NotBeNull();
            roleInDb!.Description.Should().Be("Integration Test Role");
        }
    }

    [Fact(DisplayName = "PERM_INT_008 - API tạo role mới khi không có quyền")]
    public async Task CreateRole_WithoutPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Brands.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new CreateRoleCommand
        {
            RoleName = $"Unauthorized_{uniqueId}",
            Description = "Should fail",
            Permissions = [PermissionsList.Brands.View]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName, CancellationToken.None)
            .ConfigureAwait(true);
        roleInDb.Should().BeNull();
    }

    [Fact(DisplayName = "PERM_INT_009 - API tạo role với tên trùng lặp")]
    public async Task CreateRole_DuplicateName_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Roles.Create]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Pre-create role
        var roleName = $"Duplicate{uniqueId}";
        await CreateRoleWithPermissionsInternalAsync(roleName, []);

        var request = new CreateRoleCommand
        {
            RoleName = roleName,
            Description = "Duplicate attempt",
            Permissions = [PermissionsList.Products.View]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(true);
        content.Should().Contain("already exists");
    }

    [Fact(DisplayName = "PERM_INT_010 - API cập nhật permissions của role thành công")]
    public async Task UpdateRolePermissions_ValidData_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var roleName = $"TestRole_{uniqueId}";
        await CreateRoleWithPermissionsInternalAsync(roleName, [PermissionsList.Brands.View, PermissionsList.Brands.Create]);

        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Roles.Edit]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Ensure update permissions exist
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await EnsurePermissionExistsAsync(db, PermissionsList.Products.View);
            await EnsurePermissionExistsAsync(db, PermissionsList.Products.Create);
            await EnsurePermissionExistsAsync(db, PermissionsList.Products.Edit);
        }

        // Create a dummy role to hold the permissions we are about to remove from the main test role
        // This prevents the "Cannot remove last role assignment" validation error
        await CreateRoleWithPermissionsInternalAsync(
            $"DummyHolder_{uniqueId}", 
            [PermissionsList.Brands.View, PermissionsList.Brands.Create]);

        var request = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommand
        {
            Permissions = [PermissionsList.Products.View, PermissionsList.Products.Create, PermissionsList.Products.Edit]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/Permission/roles/{roleName}", request)
            .ConfigureAwait(true);

        if (response.StatusCode != HttpStatusCode.OK)
        {
             var error = await response.Content.ReadAsStringAsync();
             throw new Exception($"API Failed with {response.StatusCode}: {error}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var verifyScope = _factory.Services.CreateScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var roleInDb = await db.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Name == roleName, CancellationToken.None)
                .ConfigureAwait(true);
            roleInDb.Should().NotBeNull();
            roleInDb!.RolePermissions.Should().HaveCount(3);
        }
    }

    [Fact(DisplayName = "PERM_INT_011 - API cập nhật role (description) thành công")]
    public async Task UpdateRole_UpdateDescription_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var roleName = $"TestRole_{uniqueId}";
        await CreateRoleWithPermissionsInternalAsync(roleName, [PermissionsList.Brands.View], "Original Description");

        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Roles.Edit]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommand
        {
            Description = "Updated Description"
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/Permission/roles/{roleName}", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName, CancellationToken.None)
            .ConfigureAwait(true);
        roleInDb.Should().NotBeNull();
        roleInDb!.Description.Should().Be("Updated Description");
    }

    [Fact(DisplayName = "PERM_INT_012 - API xóa role thành công")]
    public async Task DeleteRole_ValidRole_DeletesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var roleName = $"TestRole_{uniqueId}";
        await CreateRoleWithPermissionsInternalAsync(roleName, [PermissionsList.Brands.View]);

        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Roles.Delete]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.DeleteAsync($"/api/v1/Permission/roles/{roleName}").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName, CancellationToken.None)
            .ConfigureAwait(true);
        roleInDb.Should().BeNull();
    }

    [Fact(DisplayName = "PERM_INT_013 - API xóa role khi không có quyền")]
    public async Task DeleteRole_WithoutPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var roleName = $"TestRole_{uniqueId}";
        await CreateRoleWithPermissionsInternalAsync(roleName, [PermissionsList.Brands.View]);

        var username = $"user_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Brands.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.DeleteAsync($"/api/v1/Permission/roles/{roleName}", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName, CancellationToken.None)
            .ConfigureAwait(true);
        roleInDb.Should().NotBeNull();
    }

    [Fact(DisplayName = "PERM_INT_014 - API xóa nhiều roles thành công")]
    public async Task DeleteMultipleRoles_ValidRoles_DeletesAllSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await CreateRoleWithPermissionsInternalAsync($"Role1_{uniqueId}", [PermissionsList.Brands.View]);
        await CreateRoleWithPermissionsInternalAsync($"Role2_{uniqueId}", [PermissionsList.Products.View]);
        await CreateRoleWithPermissionsInternalAsync($"Role3_{uniqueId}", [PermissionsList.Files.View]);

        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Roles.Delete]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new List<string> { $"Role1_{uniqueId}", $"Role2_{uniqueId}", $"Role3_{uniqueId}" };

        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles/delete-multiple", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<RoleDeleteResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Message.Should().Contain("3");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var rolesInDb = await db.Roles
            .Where(r => r.Name!.Contains(uniqueId))
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(true);
        rolesInDb.Should().BeEmpty();
    }

    [Fact(DisplayName = "PERM_INT_015 - API lấy tất cả roles")]
    public async Task GetAllRoles_WithViewPermission_ReturnsAllRoles()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await CreateRoleWithPermissionsInternalAsync($"Role1_{uniqueId}", [PermissionsList.Brands.View]);
        await CreateRoleWithPermissionsInternalAsync($"Role2_{uniqueId}", [PermissionsList.Products.View]);

        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, "Password123!", [PermissionsList.Roles.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync("/api/v1/Permission/roles").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<List<RoleSelectResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content.Should().Contain(r => r.Name == $"Role1_{uniqueId}");
        content.Should().Contain(r => r.Name == $"Role2_{uniqueId}");
    }

    private async Task CreateRoleWithPermissionsInternalAsync(
        string roleName,
        string[] permissionNames,
        string? description = null)
    {
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        if (!await roleManager.RoleExistsAsync(roleName))
        {
             var role = new ApplicationRole { Name = roleName, Description = description ?? $"Test role {roleName}" };
             await roleManager.CreateAsync(role).ConfigureAwait(true);
        }

        var roleEntity = await roleManager.FindByNameAsync(roleName);

        foreach(var permName in permissionNames)
        {
            var permission = await EnsurePermissionExistsAsync(db, permName);

            if (!await db.RolePermissions.AnyAsync(rp => rp.RoleId == roleEntity!.Id && rp.PermissionId == permission.Id))
            {
                db.RolePermissions.Add(new RolePermission { RoleId = roleEntity!.Id, PermissionId = permission.Id });
            }
        }

        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
    }

    private async Task<Permission> EnsurePermissionExistsAsync(ApplicationDBContext db, string permName)
    {
        var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Name == permName);
        if (permission == null)
        {
            permission = new Permission { Name = permName };
            db.Permissions.Add(permission);
            try
            {
                await db.SaveChangesAsync();
            }
            catch
            {
                // Handle concurrency: another thread might have created it
                db.Entry(permission).State = EntityState.Detached;
                permission = await db.Permissions.FirstOrDefaultAsync(p => p.Name == permName);
            }
        }
        return permission!;
    }
}
