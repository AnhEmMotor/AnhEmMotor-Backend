using Application.ApiContracts.Auth.Responses;
using Application.Features.Auth.Commands.Login;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace IntegrationTests;

public static class IntegrationTestHelper
{
    public static async Task<ApplicationUser> CreateUserWithPermissionsAsync(
        IServiceProvider services,
        string username,
        string password,
        List<string> permissions,
        string? email = null,
        bool isLocked = false,
        string? roleName = null,
        DateTimeOffset? deletedAt = null)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // 1. Create Role
        // If roleName is not provided, generate a unique one for isolation
        var targetRoleName = roleName ?? $"TestRole_{Guid.NewGuid()}";
        
        if (!await roleManager.RoleExistsAsync(targetRoleName))
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole { Name = targetRoleName });
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to create role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
        var role = await roleManager.FindByNameAsync(targetRoleName);

        // 2. Assign permissions to the Role
        foreach (var permissionName in permissions)
        {
            var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
            if (permission == null)
            {
                permission = new Permission { Name = permissionName };
                db.Permissions.Add(permission);
                await db.SaveChangesAsync();
            }

            var existingRolePermission = await db.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == role!.Id && rp.PermissionId == permission.Id);

            if (existingRolePermission == null)
            {
                db.RolePermissions.Add(new RolePermission { RoleId = role!.Id, PermissionId = permission.Id });
                await db.SaveChangesAsync();
            }
        }

        // 3. Create User
        var userEmail = email ?? $"{username}@example.com";
        var user = new ApplicationUser
        {
            UserName = username,
            Email = userEmail,
            FullName = $"Test User {username}",
            Status = isLocked ? UserStatus.Banned : UserStatus.Active,
            DeletedAt = deletedAt
        };

        var userResult = await userManager.CreateAsync(user, password);
        if (!userResult.Succeeded)
        {
             // Check if it's password validation error
             var passwordErrors = userResult.Errors.Where(e => e.Code.StartsWith("Password"));
             if (passwordErrors.Any())
             {
                 throw new Exception($"Password validation failed: {string.Join(", ", passwordErrors.Select(e => e.Description))}");
             }
             throw new Exception($"Failed to create user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
        }

        // 4. Assign Role to User
        var addToRoleResult = await userManager.AddToRoleAsync(user, targetRoleName);
        if (!addToRoleResult.Succeeded)
        {
            throw new Exception($"Failed to assign role to user: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
        }

        // Return a fresh user object from DB to ensure everything is persited and up-to-date
        return await userManager.FindByNameAsync(username) 
            ?? throw new Exception("User created but not found.");
    }

    public static async Task<LoginResponse> AuthenticateAsync(
        HttpClient client,
        string username,
        string password)
    {
        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new LoginCommand { UsernameOrEmail = username, Password = password });

        loginResponse.EnsureSuccessStatusCode();

        return await loginResponse.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new Exception("Failed to deserialize login response");
    }
}
