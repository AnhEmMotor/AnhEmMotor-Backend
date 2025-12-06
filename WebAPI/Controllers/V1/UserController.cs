using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Asp.Versioning;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
[ApiController]
public class UserController(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Lấy thông tin người dùng hiện tại từ JWT
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid user token." }] });
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
        }

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new UserResponse()
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
    /// Đổi thông tin người dùng hiện tại từ JWT
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest model)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!GenderStatus.IsValid(model.Gender))
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid gender. Please check again." }] });
        }

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid user token." }] });
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
        }

        if (!string.IsNullOrWhiteSpace(model.FullName))
        {
            user.FullName = model.FullName;
        }

        if (!string.IsNullOrWhiteSpace(model.Gender))
        {
            user.Gender = model.Gender;
        }

        if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
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

        return Ok(new UserResponse()
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
    /// Đổi mật khẩu người dùng hiện tại từ JWT
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordUserByUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePasswordCurrentUser([FromBody] ChangePasswordRequest model)
    {
        if (string.Compare(model.CurrentPassword, model.NewPassword) == 0)
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "New password can not dupplicate current password. " }] });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid user token." }] });
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
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

        return Ok(new ChangePasswordUserByUserResponse() { Message = "Password changed successfully." });
    }

    /// <summary>
    /// Xoá tài khoản của người dùng từ JWT
    /// </summary>
    [HttpPost("delete-account")]
    [Authorize]
    [ProducesResponseType(typeof(DeleteUserByUserReponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCurrentUserAccount()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid user token." }] });
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
        }

        // Check if already deleted
        if (user.DeletedAt is not null)
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "This account has already been deleted." }] });
        }

        // Kiểm tra protected users
        var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers").Get<List<string>>() ?? [];
        var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

        if (!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Protected users cannot delete their account." }] });
        }

        user.DeletedAt = DateTimeOffset.UtcNow;
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

        return Ok(new DeleteUserByUserReponse()
        {
            Message = "Your account has been deleted successfully.",
        });
    }

    /// <summary>
    /// Khôi phục tài khoản người dùng (soft delete recovery - set DeletedAt to null)
    /// </summary>
    [HttpPost("{userId:guid}/restore")]
    [ProducesResponseType(typeof(RestoreUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreUserAccount(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User not found." }] });
        }

        if (user.DeletedAt is null)
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User account is not deleted." }] });
        }

        // Chỉ có thể khôi phục nếu Status là Active
        if (user.Status != UserStatus.Active)
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = $"Cannot restore user with status '{user.Status}'. User status must be Active." }] });
        }

        user.DeletedAt = null;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { result.Errors });
        }

        return Ok(new RestoreUserResponse()
        {
            Message = "User account has been restored successfully.",
        });
    }
}
