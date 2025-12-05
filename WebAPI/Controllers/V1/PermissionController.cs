using Application.ApiContracts.Auth;
using Asp.Versioning;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using Infrastructure.Authorization;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý quyền hạn và vai trò
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Controller quản lý quyền hạn và vai trò")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
[ApiController]
public class PermissionController(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDBContext context,
    IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Lấy tất cả các permissions có trong hệ thống với mô tả và trạng thái truy cập của người dùng hiện tại
    /// </summary>
    [HttpGet("permissions")]
    [HasPermission(PermissionsList.Roles.View)]
    public async Task<IActionResult> GetAllPermissions()
    {
        // Lấy user hiện tại
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ApplicationUser? currentUser = null;
        
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            currentUser = await userManager.FindByIdAsync(userId.ToString());
        }

        // Lấy permissions từ class Permissions
        var permissionDefinitions = typeof(PermissionsList)
            .GetNestedTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
            .Select(fieldInfo => new
            {
                Category = fieldInfo.DeclaringType?.Name ?? "Unknown",
                Key = fieldInfo.Name,
                Permission = fieldInfo.GetRawConstantValue() as string
            })
            .Where(p => p.Permission is not null)
            .ToList();

        // Lấy permissions đã được gán cho user (nếu user đã đăng nhập)
        var userPermissions = new HashSet<string>();
        if (currentUser is not null)
        {
            var userRoles = await userManager.GetRolesAsync(currentUser);
            var roleEntities = await context.Roles
                .Where(r => userRoles.Contains(r.Name!))
                .ToListAsync();

            var roleIds = roleEntities.Select(r => r.Id).ToList();
            userPermissions = [.. (await context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission!.Name)
                .ToListAsync())];
        }

        var result = permissionDefinitions
            .GroupBy(p => p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p =>
                {
                    var metadata = PermissionsList.GetMetadata(p.Permission!);
                    return new
                    {
                        p.Key,
                        p.Permission,
                        DisplayName = metadata?.DisplayName ?? p.Key,
                        metadata?.Description,
                        HasAccess = userPermissions.Contains(p.Permission!)
                    };
                }).ToList());

        return Ok(result);
    }

    /// <summary>
    /// Lấy các quyền của người dùng hiện tại
    /// </summary>
    [HttpGet("my-permissions")]
    [Authorize]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Message = "Invalid user token." });
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound(new { Message = "User not found." });
        }

        var userRoles = await userManager.GetRolesAsync(user);
        var roleEntities = await context.Roles
            .Where(r => userRoles.Contains(r.Name!))
            .ToListAsync();

        var roleIds = roleEntities.Select(r => r.Id).ToList();
        var userPermissionNames = await context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync();

        var userPermissions = userPermissionNames
            .Select(p => new
            {
                Name = p,
                Metadata = PermissionsList.GetMetadata(p)
            })
            .Select(p => new
            {
                p.Name,
                DisplayName = p.Metadata?.DisplayName ?? p.Name,
                p.Metadata?.Description
            })
            .ToList();

        return Ok(new
        {
            UserId = userId,
            user.UserName,
            Roles = userRoles,
            Permissions = userPermissions
        });
    }

    /// <summary>
    /// Lấy các quyền của một vai trò cụ thể
    /// </summary>
    [HttpGet("roles/{roleName}/permissions")]
    [HasPermission(PermissionsList.Roles.View)]
    public async Task<IActionResult> GetRolePermissions(string roleName)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            return NotFound(new { Message = "Role not found." });
        }

        var permissions = await context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .ToListAsync();

        var permissionsWithMetadata = permissions
            .Select(p => new
            {
                Name = p,
                Metadata = PermissionsList.GetMetadata(p)
            })
            .Select(p => new
            {
                p.Name,
                DisplayName = p.Metadata?.DisplayName ?? p.Name,
                p.Metadata?.Description
            })
            .ToList();

        return Ok(new { RoleName = roleName, Permissions = permissionsWithMetadata });
    }

    /// <summary>
    /// Cập nhật quyền cho một vai trò
    /// </summary>
    [HttpPut("roles/{roleName}/permissions")]
    [HasPermission(PermissionsList.Roles.AssignPermissions)]
    public async Task<IActionResult> UpdateRolePermissions(
        string roleName,
        [FromBody] UpdateRolePermissionsRequest model)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            return NotFound(new { Message = "Role not found." });
        }

        // Lấy tất cả các permissions hợp lệ
        var allPermissions = typeof(PermissionsList)
            .GetNestedTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
            .Select(fieldInfo => fieldInfo.GetRawConstantValue() as string)
            .Where(permission => permission is not null)
            .ToList();

        // Kiểm tra permissions không hợp lệ
        var invalidPermissions = model.Permissions.Except(allPermissions!).ToList();
        if (invalidPermissions.Count != 0)
        {
            return BadRequest(new
            {
                Message = $"Invalid permissions: {string.Join(", ", invalidPermissions)}"
            });
        }

        // Lấy permissions hiện tại của role
        var currentRolePermissions = await context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Include(rp => rp.Permission)
            .ToListAsync();

        var currentPermissionNames = currentRolePermissions
            .Where(rp => rp.Permission is not null)
            .Select(rp => rp.Permission!.Name)
            .ToHashSet();

        // Xác định permissions cần xóa
        var permissionsToRemove = currentRolePermissions
            .Where(rp => rp.Permission is not null && !model.Permissions.Contains(rp.Permission.Name))
            .ToList();

        // Xác định permissions cần thêm
        var permissionNamesToAdd = model.Permissions
            .Where(pName => !currentPermissionNames.Contains(pName))
            .ToList();

        // Kiểm tra không được xóa permission cuối cùng (để đảm bảo ít nhất 1 role có permission)
        if (permissionsToRemove.Count != 0)
        {
            var permissionIdsToRemove = permissionsToRemove
                .Select(rp => rp.PermissionId)
                .ToHashSet();

            var orphanedPermissionNames = await context.RolePermissions
                .Where(rp => permissionIdsToRemove.Contains(rp.PermissionId))
                .Where(rp => rp.Permission != null)
                .GroupBy(rp => rp.Permission!.Name)
                .Where(g => g.Count() == 1)
                .Select(g => g.Key)
                .ToListAsync();

            if (orphanedPermissionNames.Count != 0)
            {
                return BadRequest(new
                {
                    Message = $"Cannot remove the last role assignment for permissions: {string.Join(", ", orphanedPermissionNames)}. Assign them to another role first."
                });
            }
        }

        // Lấy permissions cần thêm từ database
        var dbPermissionsToAdd = await context.Permissions
            .Where(p => permissionNamesToAdd.Contains(p.Name))
            .ToListAsync();

        var rolePermissionsToAdd = dbPermissionsToAdd
            .Select(p => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id
            })
            .ToList();

        // Thực hiện xóa và thêm
        if (permissionsToRemove.Count != 0)
        {
            context.RolePermissions.RemoveRange(permissionsToRemove);
        }

        if (rolePermissionsToAdd.Count != 0)
        {
            await context.RolePermissions.AddRangeAsync(rolePermissionsToAdd);
        }

        if (permissionsToRemove.Count != 0 || rolePermissionsToAdd.Count != 0)
        {
            await context.SaveChangesAsync();
        }

        return Ok(new { Message = $"Permissions for role '{roleName}' updated successfully." });
    }

    /// <summary>
    /// Lấy tất cả các vai trò
    /// </summary>
    [HttpGet("roles")]
    [HasPermission(PermissionsList.Roles.View)]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await roleManager.Roles
            .Select(r => new
            {
                r.Id,
                r.Name
            })
            .ToListAsync();

        return Ok(roles);
    }

    /// <summary>
    /// Tạo vai trò mới
    /// </summary>
    [HttpPost("roles")]
    [HasPermission(PermissionsList.Roles.Create)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest model)
    {
        var roleExists = await roleManager.RoleExistsAsync(model.RoleName);
        if (roleExists)
        {
            return BadRequest(new { Message = "Role already exists." });
        }

        var result = await roleManager.CreateAsync(new ApplicationRole { Name = model.RoleName });
        if (!result.Succeeded)
        {
            return BadRequest(new { result.Errors });
        }

        return Ok(new { Message = $"Role '{model.RoleName}' created successfully." });
    }

    /// <summary>
    /// Xóa vai trò (hard delete)
    /// </summary>
    [HttpDelete("roles/{roleName}")]
    [HasPermission(PermissionsList.Roles.Delete)]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            return NotFound(new { Message = "Role not found." });
        }

        // Kiểm tra SuperRoles
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];
        if (superRoles.Contains(roleName))
        {
            return BadRequest(new { Message = "Cannot delete SuperRole." });
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return BadRequest(new { result.Errors });
        }

        return Ok(new { Message = $"Role '{roleName}' deleted successfully." });
    }

    /// <summary>
    /// Xóa nhiều vai trò (hard delete)
    /// </summary>
    [HttpPost("roles/delete-multiple")]
    [HasPermission(PermissionsList.Roles.Delete)]
    public async Task<IActionResult> DeleteMultipleRoles([FromBody] List<string> roleNames)
    {
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];

        var deletedCount = 0;
        var skippedCount = 0;

        foreach (var roleName in roleNames)
        {
            if (superRoles.Contains(roleName))
            {
                skippedCount++;
                continue;
            }

            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                skippedCount++;
                continue;
            }

            var result = await roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                deletedCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        return Ok(new
        {
            Message = $"Deleted {deletedCount} role(s) successfully.",
            DeletedCount = deletedCount,
            SkippedCount = skippedCount
        });
    }
}

/// <summary>
/// DTO cho tạo role mới
/// </summary>
public class CreateRoleRequest
{
    /// <summary>
    /// Gets or sets the name of the role associated with the user or entity.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
}
