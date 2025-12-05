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
    /// Lấy các quyền của một người dùng theo User ID
    /// </summary>
    [HttpGet("users/{userId:guid}/permissions")]
    [HasPermission(PermissionsList.Users.View)]
    public async Task<IActionResult> GetUserPermissionsById(Guid userId)
    {
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
            user.Email,
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

        bool isRoleInfoUpdated = false;
        if (model.Description is not null && role.Description != model.Description)
        {
            role.Description = model.Description;
            isRoleInfoUpdated = true;
        }

        if (model.Permissions.Count != 0)
        {
            var allPermissions = typeof(PermissionsList)
            .GetNestedTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
            .Select(fieldInfo => fieldInfo.GetRawConstantValue() as string)
            .Where(permission => permission is not null)
            .ToList();

            var invalidPermissions = model.Permissions.Except(allPermissions!).ToList();
            if (invalidPermissions.Count != 0)
            {
                return BadRequest(new
                {
                    Message = $"Invalid permissions: {string.Join(", ", invalidPermissions)}"
                });
            }

            var currentRolePermissions = await context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Include(rp => rp.Permission)
                .ToListAsync();

            var currentPermissionNames = currentRolePermissions
                .Where(rp => rp.Permission is not null)
                .Select(rp => rp.Permission!.Name)
                .ToHashSet();

            var permissionsToRemove = currentRolePermissions
                .Where(rp => rp.Permission is not null && !model.Permissions.Contains(rp.Permission.Name))
                .ToList();

            var permissionNamesToAdd = model.Permissions
                .Where(pName => !currentPermissionNames.Contains(pName))
                .ToList();

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

            if (permissionsToRemove.Count != 0)
            {
                context.RolePermissions.RemoveRange(permissionsToRemove);
                isRoleInfoUpdated = true;
            }

            if (rolePermissionsToAdd.Count != 0)
            {
                await context.RolePermissions.AddRangeAsync(rolePermissionsToAdd);
                isRoleInfoUpdated = true;
            }
        }
        

        if (isRoleInfoUpdated)
        {
            await context.SaveChangesAsync();
        }

        return Ok(new { Message = $"Role '{roleName}' updated successfully." });
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
    /// Tạo vai trò mới với các quyền được gán
    /// </summary>
    [HttpPost("roles")]
    [HasPermission(PermissionsList.Roles.Create)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest model)
    {
        // Kiểm tra role đã tồn tại
        var roleExists = await roleManager.RoleExistsAsync(model.RoleName);
        if (roleExists)
        {
            return BadRequest(new { Message = "Role already exists." });
        }

        // Kiểm tra permissions không rỗng
        if (model.Permissions is null || model.Permissions.Count == 0)
        {
            return BadRequest(new { Message = "At least one permission must be assigned to the role." });
        }

        // Lấy tất cả permissions hợp lệ
        var allPermissions = typeof(PermissionsList)
            .GetNestedTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
            .Select(fieldInfo => fieldInfo.GetRawConstantValue() as string)
            .Where(permission => permission is not null)
            .ToList();

        // Kiểm tra permissions hợp lệ
        var invalidPermissions = model.Permissions.Except(allPermissions!).ToList();
        if (invalidPermissions.Count != 0)
        {
            return BadRequest(new
            {
                Message = $"Invalid permissions: {string.Join(", ", invalidPermissions)}"
            });
        }

        // Tạo role
        var role = new ApplicationRole
        {
            Name = model.RoleName,
            Description = model.Description
        };

        var createResult = await roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            return BadRequest(new { createResult.Errors });
        }

        // Gán permissions cho role
        var permissionsInDb = await context.Permissions
            .Where(p => model.Permissions.Contains(p.Name))
            .ToListAsync();

        var rolePermissions = permissionsInDb
            .Select(p => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = p.Id
            })
            .ToList();

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();

        return Ok(new
        {
            Message = $"Role '{model.RoleName}' created successfully with {rolePermissions.Count} permission(s).",
            RoleId = role.Id,
            RoleName = role.Name,
            Description = role.Description,
            Permissions = model.Permissions
        });
    }

    /// <summary>
    /// Cập nhật thông tin vai trò (chỉ cập nhật Description nếu được cung cấp)
    /// </summary>
    [HttpPut("roles/{roleName}")]
    [HasPermission(PermissionsList.Roles.Edit)]
    public async Task<IActionResult> UpdateRole(string roleName, [FromBody] UpdateRoleRequest model)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            return NotFound(new { Message = "Role not found." });
        }

        // Chỉ cập nhật Description nếu được cung cấp
        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            role.Description = model.Description;
        }

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            return BadRequest(new { result.Errors });
        }

        return Ok(new
        {
            Message = $"Role '{roleName}' updated successfully.",
            RoleId = role.Id,
            RoleName = role.Name,
            Description = role.Description
        });
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

        // Kiểm tra không có user nào có role này
        var usersWithRole = await userManager.GetUsersInRoleAsync(roleName);
        if (usersWithRole.Count > 0)
        {
            return BadRequest(new
            {
                Message = $"Cannot delete role '{roleName}' because {usersWithRole.Count} user(s) have this role."
            });
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
        var skippedRoles = new List<string>();
        var deletedCount = 0;

        // Kiểm tra trước khi xóa
        foreach (var roleName in roleNames)
        {
            // Kiểm tra SuperRoles
            if (superRoles.Contains(roleName))
            {
                skippedRoles.Add($"{roleName} (SuperRole)");
                continue;
            }

            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                skippedRoles.Add($"{roleName} (Not found)");
                continue;
            }

            // Kiểm tra có user nào có role này không
            var usersWithRole = await userManager.GetUsersInRoleAsync(roleName);
            if (usersWithRole.Count > 0)
            {
                skippedRoles.Add($"{roleName} ({usersWithRole.Count} user(s) assigned)");
                continue;
            }
        }

        // Nếu có skipped roles, báo lỗi
        if (skippedRoles.Count > 0)
        {
            return BadRequest(new
            {
                Message = "Cannot delete some roles due to validation errors.",
                SkippedRoles = skippedRoles
            });
        }

        // Xóa tất cả roles
        foreach (var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    deletedCount++;
                }
            }
        }

        return Ok(new
        {
            Message = $"Deleted {deletedCount} role(s) successfully.",
            DeletedCount = deletedCount
        });
    }
}

/// <summary>
/// DTO cập nhật vai trò
/// </summary>
public class UpdateRoleRequest
{
    /// <summary>
    /// Mô tả của vai trò (tuỳ chọn - nếu không truyền thì giữ nguyên)
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO cho tạo role mới
/// </summary>
public class CreateRoleRequest
{
    /// <summary>
    /// Tên vai trò
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả của vai trò (tuỳ chọn)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Danh sách quyền cho vai trò (bắt buộc - phải có ít nhất 1 quyền)
    /// </summary>
    public List<string> Permissions { get; set; } = [];
}
