using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Requests;
using Application.ApiContracts.UserManager.Responses;
using Application.Features.UserManager.Queries.GetUsersList;
using Asp.Versioning;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using Domain.Shared;
using Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý người dùng (Chỉ có người dùng có quyền mới được vào đây)
/// </summary>
/// <remarks>This controller enforces business rules to prevent modification or deletion of protected
/// users and roles, such as SuperRoles and users listed in the protected configuration. All endpoints require
/// specific permissions and may return error responses if protection rules are violated. API versioning is
/// supported via the route template. Thread safety is managed by ASP.NET Core's request handling; concurrent
/// requests may result in race conditions if user or role state changes rapidly.</remarks>
/// <param name="mediator">The MediatR mediator used to send queries and commands.</param>
/// <param name="userManager">The user manager used to perform operations on user accounts, such as retrieval, update, deletion, and role
/// management.</param>
/// <param name="roleManager">The role manager used to manage roles and their permissions.</param>
/// <param name="configuration">The application configuration used to access protected user and role settings that affect authorization and
/// deletion logic.</param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý người dùng (Chỉ có người dùng có quyền mới được vào đây)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
[ApiController]
public class UserManagerController(
    IMediator mediator,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách tất cả người dùng (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [HasPermission(PermissionsList.Users.View)]
    [ProducesResponseType(typeof(PagedResult<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy thông tin người dùng theo ID
    /// </summary>
    [HttpGet("{userId:guid}")]
    [HasPermission(PermissionsList.Users.View)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if(user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
        }

        var roles = await userManager.GetRolesAsync(user);

        return Ok(
            new UserResponse()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Status = user.Status,
                DeletedAt = user.DeletedAt,
                Roles = roles
            });
    }

    /// <summary>
    /// Cập nhật thông tin người dùng
    /// </summary>
    [HttpPut("{userId:guid}")]
    [HasPermission(PermissionsList.Users.Edit)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest model)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if(user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
        }

        if(!string.IsNullOrWhiteSpace(model.FullName))
        {
            user.FullName = model.FullName;
        }

        // Validate Gender if provided
        if(!string.IsNullOrWhiteSpace(model.Gender))
        {
            if(!GenderStatus.IsValid(model.Gender))
            {
                return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = $"Invalid gender value. Allowed values: {string.Join(", ", GenderStatus.All)}" }] });
            }
            user.Gender = model.Gender;
        }

        if(!string.IsNullOrWhiteSpace(model.PhoneNumber))
        {
            user.PhoneNumber = model.PhoneNumber;
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            ErrorResponse error = new();
            foreach (var identityError in result.Errors)
            {
                string fieldName = IdentityHelper.GetFieldForIdentityError(identityError.Code);

                error.Errors.Add(new ErrorDetail()
                {
                    Field = fieldName,
                    Message = identityError.Description
                });
            }
            return BadRequest(error);
        }

        var roles = await userManager.GetRolesAsync(user);

        return Ok(
            new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Status = user.Status,
                DeletedAt = user.DeletedAt,
                Roles = roles
            });
    }

    /// <summary>
    /// Đổi mật khẩu người dùng theo ID
    /// </summary>
    [HttpPost("{userId:guid}/change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest model)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid user token." }] });
        }

        if(currentUserGuid != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid user token." }] });
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if(user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
        }

        // Check old password != new password
        if(model.CurrentPassword == model.NewPassword)
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "New password must be different from current password." }] });
        }

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            ErrorResponse error = new();
            foreach (var identityError in result.Errors)
            {
                string fieldName = IdentityHelper.GetFieldForIdentityError(identityError.Code);

                error.Errors.Add(new ErrorDetail()
                {
                    Field = fieldName,
                    Message = identityError.Description
                });
            }
            return BadRequest(error);
        }

        return Ok(new ChangePasswordByManagerResponse() { Message = "Password changed successfully." });
    }

    /// <summary>
    /// Gán roles cho người dùng
    /// </summary>
    [HttpPost("{userId:guid}/assign-roles")]
    [ProducesResponseType(typeof(AssignRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [HasPermission(PermissionsList.Users.AssignRoles)]
    public async Task<IActionResult> AssignRoles(Guid userId, [FromBody] AssignRolesRequest model)
    {
        foreach (string role in model.RoleNames)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Role names cannot contain empty or whitespace values." }] });
            }
            if (UserStatus.IsValid(role))
            {
                return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Role names not vaild." }] });
            }
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if(user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
        }

        // Validate all roles exist BEFORE assignment
        var invalidRoles = new List<string>();
        foreach(var roleName in model.RoleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if(!roleExists)
            {
                invalidRoles.Add(roleName);
            }
        }

        if(invalidRoles.Count > 0)
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = $"The following roles do not exist: {string.Join(", ", invalidRoles)}" }] });
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
                    return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = $"Cannot remove SuperRole '{roleToRemove}' from user. This is the last user with this role." }] });
                }
            }
        }

        if(currentRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                ErrorResponse error = new();
                foreach (var identityError in removeResult.Errors)
                {
                    string fieldName = IdentityHelper.GetFieldForIdentityError(identityError.Code);

                    error.Errors.Add(new ErrorDetail()
                    {
                        Field = fieldName,
                        Message = identityError.Description
                    });
                }
                return BadRequest(error);
            }
        }

        if(model.RoleNames.Count > 0)
        {
            var addResult = await userManager.AddToRolesAsync(user, model.RoleNames);
            if (!addResult.Succeeded)
            {
                ErrorResponse error = new();
                foreach (var identityError in addResult.Errors)
                {
                    string fieldName = IdentityHelper.GetFieldForIdentityError(identityError.Code);

                    error.Errors.Add(new ErrorDetail()
                    {
                        Field = fieldName,
                        Message = identityError.Description
                    });
                }
                return BadRequest(error);
            }
        }

        var updatedRoles = await userManager.GetRolesAsync(user);

        return Ok(new AssignRoleResponse() { Id = user.Id, UserName = user.UserName, Email = user.Email, FullName =user.FullName, Roles = updatedRoles });
    }

    /// <summary>
    /// Thay đổi trạng thái của người dùng
    /// </summary>
    [HttpPatch("{userId:guid}/status")]
    [HasPermission(PermissionsList.Users.Edit)]
    [ProducesResponseType(typeof(ChangeStatusByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeUserStatus(Guid userId, [FromBody] ChangeUserStatusRequest model)
    {
        if (!UserStatus.IsValid(model.Status))
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Status not vaild, please check." }] });
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if(user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
        }

        if(model.Status == UserStatus.Inactive)
        {
            var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers")
                    .Get<List<string>>() ??
                [];
            var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

            if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
            {
                return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Cannot deactivate protected user." }] });
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
                        return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = $"Cannot deactivate user. This is the last active user with SuperRole '{userRole}'." }] });
                    }
                }
            }
        }

        user.Status = model.Status;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            ErrorResponse error = new();
            foreach (var identityError in result.Errors)
            {
                string fieldName = IdentityHelper.GetFieldForIdentityError(identityError.Code);

                error.Errors.Add(new ErrorDetail()
                {
                    Field = fieldName,
                    Message = identityError.Description
                });
            }
            return BadRequest(error);
        }

        return Ok(
            new ChangeStatusByManagerResponse()
            {
                Message = $"User status changed to {model.Status} successfully.",
            });
    }


    /// <summary>
    /// Thay đổi trạng thái của nhiều người dùng 
    /// </summary>
    [HttpPatch("status")]
    [ProducesResponseType(typeof(ChangeStatusMultiUserByManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [HasPermission(PermissionsList.Users.Edit)]
    public async Task<IActionResult> ChangeMultipleUsersStatus([FromBody] ChangeMultipleUsersStatusRequest model)
    {
        if (!UserStatus.IsValid(model.Status))
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Status not vaild, please check." }] });
        }
        var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers")
                .Get<List<string>>() ??
            [];
        var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ??
            [];

        var usersToUpdate = new List<ApplicationUser>();
        var errorMessages = new ErrorResponse();

        foreach(var userId in model.UserIds)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if(user is null)
            {
                errorMessages.Errors.Add(new ErrorDetail() { Message = $"User {userId} not found." });
                continue;
            }

            if(model.Status == UserStatus.Inactive)
            {
                if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
                {
                    errorMessages.Errors.Add(new ErrorDetail() { Message = $"User {user.Email} is protected and cannot be deactivated." });
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
                            errorMessages.Errors.Add(new ErrorDetail() { Message = $"User {user.Email} is protected and cannot be deactivated." });
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

        if(errorMessages.Errors.Count > 0)
        {
            return BadRequest(errorMessages);
        }

        if(usersToUpdate.Count == 0)
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "No valid users to update." }] });
        }

        var updatedCount = 0;
        var failedUpdates = new ErrorResponse();
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
                failedUpdates.Errors.Add(new ErrorDetail() { Message = $"User {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}" });
            } else
            {
                updatedCount++;
            }
        }

        if(failedUpdates.Errors.Count > 0)
        {
            foreach(var user in usersToUpdate.Take(updatedCount))
            {
                user.Status = originalStatuses[user.Id];
                await userManager.UpdateAsync(user);
            }

            return BadRequest(failedUpdates);
        }

        return Ok(
            new ChangeStatusMultiUserByManagerResponse()
            {
                Message = $"{updatedCount} user(s) status changed to {model.Status} successfully.",
            });
    }
}
