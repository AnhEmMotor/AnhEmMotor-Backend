using Domain.Constants.Permission;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class HelperVerificationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public HelperVerificationTest(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Helper_001 - Create User with Permissions Success")]
    public async Task Helper_001_CreateUserWithPermissions_Success()
    {
        var username = "helper_test_user";
        var password = "Password123!";
        var permissions = new List<string> { PermissionsList.Users.View, PermissionsList.Products.Create };

        await IntegrationTestHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            permissions);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Verify User
        var user = await userManager.FindByNameAsync(username);
        user.Should().NotBeNull();
        user!.Email.Should().Be($"{username}@example.com");
        user.Status.Should().Be(UserStatus.Active);

        // Verify Roles
        var roles = await userManager.GetRolesAsync(user);
        roles.Should().NotBeEmpty();
        var roleName = roles.First();

        var role = await db.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == roleName);
        
        role.Should().NotBeNull();

        // Verify Permissions
        var actualPermissions = role!.RolePermissions.Select(rp => rp.Permission!.Name).ToList();
        actualPermissions.Should().Contain(permissions);
    }

    [Fact(DisplayName = "Helper_002 - Weak Password Throws Exception")]
    public async Task Helper_002_WeakPassword_ThrowsException()
    {
        var username = "weak_pass_user";
        var password = "123"; // Too short, no special chars
        var permissions = new List<string> { PermissionsList.Users.View };

        var action = async () => await IntegrationTestHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            permissions);

        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Password validation failed*");
    }

    [Fact(DisplayName = "Helper_003 - Create Locked User Success")]
    public async Task Helper_003_CreateLockedUser_Success()
    {
        var username = "locked_user";
        var password = "Password123!";
        var permissions = new List<string> { PermissionsList.Users.View };

        await IntegrationTestHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            permissions,
            isLocked: true);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByNameAsync(username);
        user.Should().NotBeNull();
        user!.Status.Should().Be(UserStatus.Banned);
    }

    [Fact(DisplayName = "Helper_004 - Authenticate User Success")]
    public async Task Helper_004_AuthenticateUser_Success()
    {
        var username = "auth_test_user";
        var password = "Password123!";
        var permissions = new List<string> { PermissionsList.Users.View };

        await IntegrationTestHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            permissions);

        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        loginResponse.Should().NotBeNull();
        loginResponse.AccessToken.Should().NotBeNullOrEmpty();
    }
}
