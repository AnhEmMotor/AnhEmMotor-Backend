using Application.ApiContracts.User;
using Asp.Versioning;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Quản lý người dùng (Chỉ có người dùng có quyền mới được vào đây)
    /// </summary>
    /// <remarks>This controller enforces business rules to prevent modification or deletion of protected
    /// users and roles, such as SuperRoles and users listed in the protected configuration. All endpoints require
    /// specific permissions and may return error responses if protection rules are violated. API versioning is
    /// supported via the route template. Thread safety is managed by ASP.NET Core's request handling; concurrent
    /// requests may result in race conditions if user or role state changes rapidly.</remarks>
    /// <param name="userManager">The user manager used to perform operations on user accounts, such as retrieval, update, deletion, and role
    /// management.</param>
    /// <param name="configuration">The application configuration used to access protected user and role settings that affect authorization and
    /// deletion logic.</param>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý người dùng (Chỉ có người dùng có quyền mới được vào đây)")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ApiController]
    public class UserManagerController(UserManager<ApplicationUser> userManager, IConfiguration configuration) : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách tất cả người dùng
        /// </summary>
        [HttpGet]
        [HasPermission(PermissionsList.Users.View)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userManager.Users.ToListAsync();

            var userDtos = new List<object>();

            foreach(var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                userDtos.Add(
                    new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.FullName,
                        user.Gender,
                        user.PhoneNumber,
                        user.EmailConfirmed,
                        user.Status,
                        user.DeletedAt,
                        Roles = roles
                    });
            }

            return Ok(userDtos);
        }

        /// <summary>
        /// Lấy thông tin người dùng theo ID
        /// </summary>
        [HttpGet("{userId:guid}")]
        [HasPermission(PermissionsList.Users.View)]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if(user is null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var roles = await userManager.GetRolesAsync(user);

            return Ok(
                new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    user.Gender,
                    user.PhoneNumber,
                    user.EmailConfirmed,
                    user.Status,
                    user.DeletedAt,
                    Roles = roles
                });
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        [HttpPut("{userId:guid}")]
        [HasPermission(PermissionsList.Users.Edit)]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest model)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if(user is null)
            {
                return NotFound(new { Message = "User not found." });
            }

            if(!string.IsNullOrWhiteSpace(model.FullName))
            {
                user.FullName = model.FullName;
            }

            if(!string.IsNullOrWhiteSpace(model.Gender))
            {
                user.Gender = model.Gender;
            }

            if(!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                user.PhoneNumber = model.PhoneNumber;
            }

            var result = await userManager.UpdateAsync(user);
            if(!result.Succeeded)
            {
                return BadRequest(new { result.Errors });
            }

            var roles = await userManager.GetRolesAsync(user);

            return Ok(
                new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    user.Gender,
                    user.PhoneNumber,
                    user.EmailConfirmed,
                    user.Status,
                    user.DeletedAt,
                    Roles = roles
                });
        }

        /// <summary>
        /// Đổi mật khẩu người dùng theo ID
        /// </summary>
        [HttpPost("{userId:guid}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserGuid))
            {
                return Unauthorized(new { Message = "Invalid user token." });
            }

            if(currentUserGuid != userId)
            {
                return Forbid();
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            if(user is null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if(!result.Succeeded)
            {
                return BadRequest(new { result.Errors });
            }

            return Ok(new { Message = "Password changed successfully." });
        }

        /// <summary>
        /// Xóa một người dùng (hard delete - Identity không hỗ trợ soft delete)
        /// </summary>
        [HttpDelete("{userId:guid}")]
        [HasPermission(PermissionsList.Users.Delete)]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if(user is null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers")
                    .Get<List<string>>() ??
                [];
            var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

            if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
            {
                return BadRequest(new { Message = "Cannot delete protected user." });
            }

            var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ??
                [];
            var userRoles = await userManager.GetRolesAsync(user);

            foreach(var userRole in userRoles)
            {
                if(superRoles.Contains(userRole))
                {
                    var usersInRole = await userManager.GetUsersInRoleAsync(userRole);
                    if(usersInRole.Count == 1)
                    {
                        return BadRequest(
                            new { Message = $"Cannot delete user. This is the last user with SuperRole '{userRole}'." });
                    }
                }
            }

            var result = await userManager.DeleteAsync(user);
            if(!result.Succeeded)
            {
                return BadRequest(new { result.Errors });
            }

            return Ok(new { Message = "User deleted successfully." });
        }

        /// <summary>
        /// Xóa nhiều người dùng (hard delete) Quy tắc: Chỉ được phép xóa khi và chỉ khi tất cả users đều có thể được
        /// xóa
        /// </summary>
        [HttpPost("delete-multiple")]
        [HasPermission(PermissionsList.Users.Delete)]
        public async Task<IActionResult> DeleteMultipleUsers([FromBody] List<Guid> userIds)
        {
            var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers")
                    .Get<List<string>>() ??
                [];
            var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();
            var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ??
                [];

            var usersToDelete = new List<ApplicationUser>();
            var errorMessages = new List<string>();

            foreach(var userId in userIds)
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if(user is null)
                {
                    errorMessages.Add($"User {userId} not found.");
                    continue;
                }

                if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
                {
                    errorMessages.Add($"User {user.Email} is protected and cannot be deleted.");
                    continue;
                }

                var userRoles = await userManager.GetRolesAsync(user);
                var isLastInSuperRole = false;

                foreach(var userRole in userRoles)
                {
                    if(superRoles.Contains(userRole))
                    {
                        var usersInRole = await userManager.GetUsersInRoleAsync(userRole);
                        if(usersInRole.Count == 1)
                        {
                            errorMessages.Add($"User {user.UserName} is the last user with SuperRole '{userRole}'.");
                            isLastInSuperRole = true;
                            break;
                        }
                    }
                }

                if(!isLastInSuperRole)
                {
                    usersToDelete.Add(user);
                }
            }

            if(errorMessages.Count > 0)
            {
                return BadRequest(
                    new { Message = "Cannot delete users. The following errors occurred:", Errors = errorMessages });
            }

            if(usersToDelete.Count == 0)
            {
                return BadRequest(new { Message = "No valid users to delete." });
            }

            var deletedCount = 0;
            foreach(var user in usersToDelete)
            {
                var result = await userManager.DeleteAsync(user);
                if(result.Succeeded)
                {
                    deletedCount++;
                }
            }

            return Ok(new { Message = $"Deleted {deletedCount} user(s) successfully.", DeletedCount = deletedCount });
        }

        /// <summary>
        /// Gán roles cho người dùng
        /// </summary>
        [HttpPost("{userId:guid}/assign-roles")]
        [HasPermission(PermissionsList.Users.AssignRoles)]
        public async Task<IActionResult> AssignRoles(Guid userId, [FromBody] AssignRolesRequest model)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if(user is null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ??
                [];

            var rolesToRemove = currentRoles.Except(model.RoleNames).ToList();
            foreach(var roleToRemove in rolesToRemove)
            {
                if(superRoles.Contains(roleToRemove))
                {
                    var usersWithThisRole = await userManager.GetUsersInRoleAsync(roleToRemove);
                    if(usersWithThisRole.Count == 1 && usersWithThisRole[0].Id == userId)
                    {
                        return BadRequest(
                            new
                            {
                                Message = $"Cannot remove SuperRole '{roleToRemove}' from user. This is the last user with this role."
                            });
                    }
                }
            }

            if(currentRoles.Count > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                if(!removeResult.Succeeded)
                {
                    return BadRequest(new { removeResult.Errors });
                }
            }

            if(model.RoleNames.Count > 0)
            {
                var addResult = await userManager.AddToRolesAsync(user, model.RoleNames);
                if(!addResult.Succeeded)
                {
                    return BadRequest(new { addResult.Errors });
                }
            }

            var updatedRoles = await userManager.GetRolesAsync(user);

            return Ok(new { user.Id, user.UserName, user.Email, user.FullName, Roles = updatedRoles });
        }

        /// <summary>
        /// Thay đổi trạng thái của người dùng
        /// </summary>
        [HttpPatch("{userId:guid}/status")]
        [HasPermission(PermissionsList.Users.Edit)]
        public async Task<IActionResult> ChangeUserStatus(Guid userId, [FromBody] ChangeUserStatusRequest model)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if(user is null)
            {
                return NotFound(new { Message = "User not found." });
            }

            if(model.Status == UserStatus.Inactive)
            {
                var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers")
                        .Get<List<string>>() ??
                    [];
                var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

                if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
                {
                    return BadRequest(new { Message = "Cannot deactivate protected user." });
                }

                var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles")
                        .Get<List<string>>() ??
                    [];
                var userRoles = await userManager.GetRolesAsync(user);

                foreach(var userRole in userRoles)
                {
                    if(superRoles.Contains(userRole))
                    {
                        var usersInRole = await userManager.GetUsersInRoleAsync(userRole);
                        var activeUsersInRole = usersInRole.Where(u => u.Status == UserStatus.Active).ToList();

                        if(activeUsersInRole.Count == 1 && activeUsersInRole[0].Id == userId)
                        {
                            return BadRequest(
                                new
                                {
                                    Message = $"Cannot deactivate user. This is the last active user with SuperRole '{userRole}'."
                                });
                        }
                    }
                }
            }

            user.Status = model.Status;
            var result = await userManager.UpdateAsync(user);
            if(!result.Succeeded)
            {
                return BadRequest(new { result.Errors });
            }

            return Ok(
                new
                {
                    Message = $"User status changed to {model.Status} successfully.",
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.Status,
                    user.DeletedAt
                });
        }


        /// <summary>
        /// Thay đổi trạng thái của nhiều người dùng Quy tắc: Chỉ được phép thay đổi khi và chỉ khi tất cả users đều có
        /// thể thay đổi được
        /// </summary>
        [HttpPatch("status")]
        [HasPermission(PermissionsList.Users.Edit)]
        public async Task<IActionResult> ChangeMultipleUsersStatus([FromBody] ChangeMultipleUsersStatusRequest model)
        {
            var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers")
                    .Get<List<string>>() ??
                [];
            var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();
            var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ??
                [];

            var usersToUpdate = new List<ApplicationUser>();
            var errorMessages = new List<string>();

            foreach(var userId in model.UserIds)
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if(user is null)
                {
                    errorMessages.Add($"User {userId} not found.");
                    continue;
                }

                if(model.Status == UserStatus.Inactive)
                {
                    if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
                    {
                        errorMessages.Add($"User {user.Email} is protected and cannot be deactivated.");
                        continue;
                    }

                    var userRoles = await userManager.GetRolesAsync(user);
                    var isLastActiveInSuperRole = false;

                    foreach(var userRole in userRoles)
                    {
                        if(superRoles.Contains(userRole))
                        {
                            var usersInRole = await userManager.GetUsersInRoleAsync(userRole);
                            var activeUsersInRole = usersInRole.Where(u => u.Status == UserStatus.Active).ToList();

                            if(activeUsersInRole.Count == 1 && activeUsersInRole[0].Id == userId)
                            {
                                errorMessages.Add(
                                    $"User {user.UserName} is the last active user with SuperRole '{userRole}'.");
                                isLastActiveInSuperRole = true;
                                break;
                            }
                        }
                    }

                    if(isLastActiveInSuperRole)
                    {
                        continue;
                    }
                }

                usersToUpdate.Add(user);
            }

            if(errorMessages.Count > 0)
            {
                return BadRequest(
                    new
                    {
                        Message = "Cannot change user status. The following errors occurred:",
                        Errors = errorMessages
                    });
            }

            if(usersToUpdate.Count == 0)
            {
                return BadRequest(new { Message = "No valid users to update." });
            }

            var updatedCount = 0;
            var failedUpdates = new List<string>();
            var originalStatuses = new Dictionary<Guid, string>();

            foreach(var user in usersToUpdate)
            {
                originalStatuses[user.Id] = user.Status;
            }

            foreach(var user in usersToUpdate)
            {
                user.Status = model.Status;
                var result = await userManager.UpdateAsync(user);

                if(!result.Succeeded)
                {
                    failedUpdates.Add(
                        $"User {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                } else
                {
                    updatedCount++;
                }
            }

            if(failedUpdates.Count > 0)
            {
                foreach(var user in usersToUpdate.Take(updatedCount))
                {
                    user.Status = originalStatuses[user.Id];
                    await userManager.UpdateAsync(user);
                }

                return BadRequest(
                    new
                    {
                        Message = "Failed to update some users. All changes have been rolled back.",
                        Errors = failedUpdates
                    });
            }

            return Ok(
                new
                {
                    Message = $"{updatedCount} user(s) status changed to {model.Status} successfully.",
                    UpdatedCount = updatedCount
                });
        }
    }
}
