using Application.ApiContracts.Permission.Responses;
using Application.Features.Permissions.Commands.CreateRole;
using Application.Features.Permissions.Commands.UpdateRole;
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

public class PermissionAndRole : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public PermissionAndRole(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    #pragma warning disable IDE0079
    #pragma warning disable CRR0035
    [Fact(DisplayName = "PERM_INT_001 - API lấy tất cả permissions trả về đầy đủ thông tin")]
    public async Task GetAllPermissions_ReturnsFullPermissionList()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var permissionsToSeed = new[] { Domain.Constants.Permission.Permissions.Brands.View, Domain.Constants.Permission.Permissions.Products.View };
            foreach (var permName in permissionsToSeed)
            {
                await EnsurePermissionExistsAsync(db, permName).ConfigureAwait(true);
            }
        }
        var response = await _client.GetAsync("/api/v1/Permission/permissions", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<Dictionary<string, List<PermissionResponse>>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Should().ContainKey("Thương hiệu");
        content!.Should().ContainKey("Sản phẩm");
        content!.Should().ContainKey("Vai trò");
        content!["Thương hiệu"].Should().NotBeEmpty();
        content["Thương hiệu"].Should()
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
            Domain.Constants.Permission.Permissions.Brands.View,
            Domain.Constants.Permission.Permissions.Brands.Create,
            Domain.Constants.Permission.Permissions.Products.View,
            Domain.Constants.Permission.Permissions.Products.Create,
            Domain.Constants.Permission.Permissions.Files.View,
            Domain.Constants.Permission.Permissions.Files.Upload
        };
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            permissions,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync(
            "/api/v1/Permission/my-permissions",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PermissionAndRoleOfUserResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.UserId.Should().Be(user.Id);
        content.Permissions.Should().HaveCount(6);
    }

    [Fact(DisplayName = "PERM_INT_003 - API lấy permissions của user hiện tại khi chưa đăng nhập")]
    public async Task GetMyPermissions_Unauthenticated_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync(
            "/api/v1/Permission/my-permissions",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "PERM_INT_004 - API lấy permissions của user khác bằng userId")]
    public async Task GetUserPermissionsById_WithViewPermission_ReturnsTargetUserPermissions()
    {
        var targetUniqueId = Guid.NewGuid().ToString("N")[..8];
        var targetUsername = $"target_{targetUniqueId}";
        var targetUser = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            targetUsername,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Products.View, Domain.Constants.Permission.Permissions.Brands.View, Domain.Constants.Permission.Permissions.Files.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var callerUniqueId = Guid.NewGuid().ToString("N")[..8];
        var callerUsername = $"caller_{callerUniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            callerUsername,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Users.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            callerUsername,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync(
            $"/api/v1/Permission/users/{targetUser.Id}/permissions",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PermissionAndRoleOfUserResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.UserId.Should().Be(targetUser.Id);
        content.Permissions.Should().HaveCount(3);
    }

    [Fact(DisplayName = "PERM_INT_005 - API lấy permissions của user khác khi không có quyền")]
    public async Task GetUserPermissionsById_WithoutPermission_ReturnsForbidden()
    {
        var targetUniqueId = Guid.NewGuid().ToString("N")[..8];
        var targetUsername = $"target_{targetUniqueId}";
        var targetUser = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            targetUsername,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Products.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var callerUniqueId = Guid.NewGuid().ToString("N")[..8];
        var callerUsername = $"caller_{callerUniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            callerUsername,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Brands.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            callerUsername,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync(
            $"/api/v1/Permission/users/{targetUser.Id}/permissions",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PERM_INT_006 - API lấy permissions của role hợp lệ")]
    public async Task GetRolePermissions_ValidRole_ReturnsRolePermissions()
    {
        var roleName = $"Manager_{Guid.NewGuid():N}";
        var roleId = await CreateRoleWithPermissionsInternalAsync(
            roleName,
            [Domain.Constants.Permission.Permissions.Brands.View, Domain.Constants.Permission.Permissions.Brands.Create, Domain.Constants.Permission.Permissions.Brands.Edit, Domain.Constants.Permission.Permissions.Brands.Delete, Domain.Constants.Permission.Permissions.Products.View])
            .ConfigureAwait(true);
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"caller_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync(
            $"/api/v1/Permission/roles/{roleId}/permissions",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<List<string>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Should().HaveCount(5);
        content!.Should().Contain(Domain.Constants.Permission.Permissions.Brands.View);
        content!.Should().Contain(Domain.Constants.Permission.Permissions.Products.View);
    }

    [Fact(DisplayName = "PERM_INT_007 - API tạo role mới thành công")]
    public async Task CreateRole_ValidData_CreatesRoleSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.Create],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await EnsurePermissionExistsAsync(db, Domain.Constants.Permission.Permissions.Brands.View).ConfigureAwait(true);
            await EnsurePermissionExistsAsync(db, Domain.Constants.Permission.Permissions.Products.View).ConfigureAwait(true);
        }
        var newRoleName = $"NewRole{uniqueId}";
        var request = new CreateRoleCommand
        {
            RoleName = newRoleName,
            Description = "Integration Test Role",
            Permissions = [Domain.Constants.Permission.Permissions.Brands.View, Domain.Constants.Permission.Permissions.Products.View]
        };
        var response = await _client.PostAsJsonAsync("/api/v1/permission/roles", request).ConfigureAwait(true);
        var contentString = await response!.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Created, contentString);
        var content = await response!.Content
            .ReadFromJsonAsync<RoleCreateResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.RoleId.Should().NotBeEmpty();
        content.RoleName.Should().Be(newRoleName);
        content.Description.Should().Be("Integration Test Role");
        content.Permissions.Should().HaveCount(2);
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await verifyDb.Roles
            .FirstOrDefaultAsync(r => string.Compare(r.Name, newRoleName) == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        roleInDb.Should().NotBeNull();
        roleInDb!.Description.Should().Be("Integration Test Role");
    }

    [Fact(DisplayName = "PERM_INT_008 - API tạo role mới khi không có quyền")]
    public async Task CreateRole_WithoutPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Brands.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var request = new CreateRoleCommand
        {
            RoleName = $"Unauthorized_{uniqueId}",
            Description = "Should fail",
            Permissions = [Domain.Constants.Permission.Permissions.Brands.View]
        };
        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles", request).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles
            .FirstOrDefaultAsync(
                r => string.Compare(r.Name, request.RoleName) == 0,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        roleInDb.Should().BeNull();
    }

    [Fact(DisplayName = "PERM_INT_009 - API tạo role với tên trùng lặp")]
    public async Task CreateRole_DuplicateName_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.Create],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var roleName = $"Duplicate{uniqueId}";
        await CreateRoleWithPermissionsInternalAsync(roleName, []).ConfigureAwait(true);
        var request = new CreateRoleCommand
        {
            RoleName = roleName,
            Description = "Duplicate attempt",
            Permissions = [Domain.Constants.Permission.Permissions.Products.View]
        };
        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles", request).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response!.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().Contain("already exists");
    }

    [Fact(DisplayName = "PERM_INT_010 - API cập nhật permissions của role thành công")]
    public async Task UpdateRolePermissions_ValidData_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var roleName = $"TestRole_{uniqueId}";
        var roleId = await CreateRoleWithPermissionsInternalAsync(
            roleName,
            [Domain.Constants.Permission.Permissions.Brands.View, Domain.Constants.Permission.Permissions.Brands.Create])
            .ConfigureAwait(true);
        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.Edit],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await EnsurePermissionExistsAsync(db, Domain.Constants.Permission.Permissions.Products.View).ConfigureAwait(true);
            await EnsurePermissionExistsAsync(db, Domain.Constants.Permission.Permissions.Products.Create).ConfigureAwait(true);
            await EnsurePermissionExistsAsync(db, Domain.Constants.Permission.Permissions.Products.Edit).ConfigureAwait(true);
        }
        await CreateRoleWithPermissionsInternalAsync(
            $"DummyHolder_{uniqueId}",
            [Domain.Constants.Permission.Permissions.Brands.View, Domain.Constants.Permission.Permissions.Brands.Create])
            .ConfigureAwait(true);
        var request = new UpdateRoleCommand
        {
            Permissions =
                [Domain.Constants.Permission.Permissions.Products.View, Domain.Constants.Permission.Permissions.Products.Create, Domain.Constants.Permission.Permissions.Products.Edit]
        };
        var response = await _client.PutAsJsonAsync($"/api/v1/Permission/roles/{roleId}", request).ConfigureAwait(true);
        if (response!.StatusCode != HttpStatusCode.OK)
        {
            var error = await response!.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            throw new Exception($"API Failed with {response!.StatusCode}: {error}");
        }
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var verifyScope = _factory.Services.CreateScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var roleInDb = await db.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => string.Compare(r.Name, roleName) == 0, TestContext.Current.CancellationToken)
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
        var roleId = await CreateRoleWithPermissionsInternalAsync(
            roleName,
            [Domain.Constants.Permission.Permissions.Brands.View],
            "Original Description")
            .ConfigureAwait(true);
        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.Edit],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var request = new UpdateRoleCommand { Description = "Updated Description" };
        var response = await _client.PutAsJsonAsync($"/api/v1/Permission/roles/{roleId}", request).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles
            .FirstOrDefaultAsync(r => string.Compare(r.Name, roleName) == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        roleInDb.Should().NotBeNull();
        roleInDb!.Description.Should().Be("Updated Description");
    }

    [Fact(DisplayName = "PERM_INT_012 - API xóa role thành công")]
    public async Task DeleteRole_ValidRole_DeletesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var roleName = $"TestRole_{uniqueId}";
        var roleId = await CreateRoleWithPermissionsInternalAsync(roleName, [Domain.Constants.Permission.Permissions.Brands.View])
            .ConfigureAwait(true);
        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.Delete],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.DeleteAsync(
            $"/api/v1/Permission/roles/{roleId}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles
            .FirstOrDefaultAsync(r => string.Compare(r.Name, roleName) == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        roleInDb.Should().BeNull();
    }

    [Fact(DisplayName = "PERM_INT_013 - API xóa role khi không có quyền")]
    public async Task DeleteRole_WithoutPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var roleName = $"TestRole_{uniqueId}";
        var roleId = await CreateRoleWithPermissionsInternalAsync(roleName, [Domain.Constants.Permission.Permissions.Brands.View])
            .ConfigureAwait(true);
        var username = $"user_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Brands.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.DeleteAsync(
            $"/api/v1/Permission/roles/{roleId}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var roleInDb = await db.Roles
            .FirstOrDefaultAsync(r => string.Compare(r.Name, roleName) == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        roleInDb.Should().NotBeNull();
    }

    [Fact(DisplayName = "PERM_INT_014 - API xóa nhiều roles thành công")]
    public async Task DeleteMultipleRoles_ValidRoles_DeletesAllSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await CreateRoleWithPermissionsInternalAsync($"Role1_{uniqueId}", [Domain.Constants.Permission.Permissions.Brands.View])
            .ConfigureAwait(true);
        await CreateRoleWithPermissionsInternalAsync($"Role2_{uniqueId}", [Domain.Constants.Permission.Permissions.Products.View])
            .ConfigureAwait(true);
        await CreateRoleWithPermissionsInternalAsync($"Role3_{uniqueId}", [Domain.Constants.Permission.Permissions.Files.View])
            .ConfigureAwait(true);
        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.Delete],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var request = new List<string> { $"Role1_{uniqueId}", $"Role2_{uniqueId}", $"Role3_{uniqueId}" };
        var response = await _client.PostAsJsonAsync("/api/v1/Permission/roles/delete-multiple", request)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<RoleDeleteResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Message.Should().Contain("3");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var rolesInDb = await db.Roles
            .Where(r => r.Name!.Contains(uniqueId))
            .ToListAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        rolesInDb.Should().BeEmpty();
    }

    [Fact(DisplayName = "PERM_INT_015 - API lấy tất cả roles")]
    public async Task GetAllRoles_WithViewPermission_ReturnsAllRoles()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await CreateRoleWithPermissionsInternalAsync($"Role1_{uniqueId}", [Domain.Constants.Permission.Permissions.Brands.View])
            .ConfigureAwait(true);
        await CreateRoleWithPermissionsInternalAsync($"Role2_{uniqueId}", [Domain.Constants.Permission.Permissions.Products.View])
            .ConfigureAwait(true);
        var username = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync("/api/v1/Permission/roles", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<RoleSelectResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().NotBeEmpty();
        content.Items.Should().Contain(r => string.Compare(r.Name, $"Role1_{uniqueId}") == 0);
        content.Items.Should().Contain(r => string.Compare(r.Name, $"Role2_{uniqueId}") == 0);
    }

    private async Task<Guid> CreateRoleWithPermissionsInternalAsync(
        string roleName,
        string[] permissionNames,
        string? description = null)
    {
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        if (!await roleManager.RoleExistsAsync(roleName).ConfigureAwait(true))
        {
            var role = new ApplicationRole { Name = roleName, Description = description ?? $"Test role {roleName}" };
            await roleManager.CreateAsync(role).ConfigureAwait(true);
        }
        var roleEntity = await roleManager.FindByNameAsync(roleName).ConfigureAwait(true);
        foreach (var permName in permissionNames)
        {
            var permission = await EnsurePermissionExistsAsync(db, permName).ConfigureAwait(true);
            if (!await db.RolePermissions
                .AnyAsync(
                    rp => rp.RoleId == roleEntity!.Id && rp.PermissionId == permission.Id,
                    TestContext.Current.CancellationToken)
                .ConfigureAwait(true))
            {
                db.RolePermissions.Add(new RolePermission { RoleId = roleEntity!.Id, PermissionId = permission.Id });
            }
        }
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        return roleEntity!.Id;
    }

    [Fact(DisplayName = "PERM_INT_016 - Lấy cấu trúc quyền hạn thành công")]
    public async Task GetPermissionStructure_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"admin_{uniqueId}";
        var password = "StrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Domain.Constants.Permission.Permissions.Roles.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync("/api/v1/Permission/structure", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var structure = await response!.Content
            .ReadFromJsonAsync<PermissionStructureResponse>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        structure.Should().NotBeNull();
        structure!.Groups.Should().NotBeEmpty();
        structure.Groups.Should().ContainKey("Sản phẩm");
        structure.Dependencies.Should().ContainKey(Domain.Constants.Permission.Permissions.Products.Create);
    }

    [Fact(DisplayName = "PERM_036 - Truy cập module HR với quyền hợp lệ")]
    public async Task GetHR_WithPermission_ReturnsOk()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"hr_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.HR.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync("/api/v1/hr/employees", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "PERM_037 - Từ chối truy cập module HR khi thiếu quyền")]
    public async Task GetHR_WithoutPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"sales_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Brands.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync("/api/v1/hr/employees", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PERM_038 - Kiểm tra quyền phê duyệt lương (Payroll)")]
    public async Task ApprovePayroll_WithoutPermission_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"hr_staff_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            "Password123!",
            [Domain.Constants.Permission.Permissions.HR.View, Domain.Constants.Permission.Permissions.Payroll.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/hr/commissions/approve-payroll",
            new { Month = 5, Year = 2026 },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "PERM_040 - Thu hồi quyền từ vai trò")]
    public async Task RevokePermissionFromRole_ValidData_RemovesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var roleName = $"Editor_{uniqueId}";
        await CreateRoleWithPermissionsInternalAsync($"OtherRole_{uniqueId}", [Domain.Constants.Permission.Permissions.News.Create])
            .ConfigureAwait(true);
        var roleId = await CreateRoleWithPermissionsInternalAsync(
            roleName,
            [Domain.Constants.Permission.Permissions.News.Create, Domain.Constants.Permission.Permissions.News.View])
            .ConfigureAwait(true);
        var adminUsername = $"admin_{uniqueId}";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminUsername,
            "Password123!",
            [Domain.Constants.Permission.Permissions.Roles.Edit],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            adminUsername,
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var request = new UpdateRoleCommand { Permissions = [Domain.Constants.Permission.Permissions.News.View] };
        var response = await HttpClientJsonExtensions.PutAsJsonAsync(
            _client,
            $"/api/v1/Permission/roles/{roleId}",
            request,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var rolePermissions = await db.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        rolePermissions.Should().HaveCount(1);
        rolePermissions.Should()
            .NotContain(
                rp => rp.Permission!.Name.Equals(Domain.Constants.Permission.Permissions.News.Create, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<Permission> EnsurePermissionExistsAsync(ApplicationDBContext db, string permName)
    {
        var permission = await db.Permissions
            .FirstOrDefaultAsync(p => string.Compare(p.Name, permName) == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (permission == null)
        {
            permission = new Permission { Name = permName };
            db.Permissions.Add(permission);
            try
            {
                await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            } catch
            {
                db.Entry(permission).State = EntityState.Detached;
                permission = await db.Permissions
                    .FirstOrDefaultAsync(
                        p => string.Compare(p.Name, permName) == 0,
                        TestContext.Current.CancellationToken)
                    .ConfigureAwait(true);
            }
        }
        return permission!;
    }
}


