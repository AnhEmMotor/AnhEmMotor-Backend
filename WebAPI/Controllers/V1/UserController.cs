using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.Features.Users.Commands.ChangePasswordCurrentUser;
using Application.Features.Users.Commands.DeleteCurrentUserAccount;
using Application.Features.Users.Commands.RestoreUserAccount;
using Application.Features.Users.Commands.UpdateCurrentUser;
using Application.Features.Users.Queries.GetCurrentUser;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
[ApiController]
public class UserController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy thông tin người dùng hiện tại từ JWT
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new GetCurrentUserQuery(userId), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Đổi thông tin người dùng hiện tại từ JWT
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDTOForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrentUser(
        [FromBody] UpdateUserRequest model,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new UpdateCurrentUserCommand(userId, model), cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Đổi mật khẩu người dùng hiện tại từ JWT
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordUserByUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePasswordCurrentUser(
        [FromBody] ChangePasswordRequest model,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new ChangePasswordCurrentUserCommand(userId, model), cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Xoá tài khoản của người dùng từ JWT
    /// </summary>
    [HttpPost("delete-account")]
    [Authorize]
    [ProducesResponseType(typeof(DeleteUserByUserReponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCurrentUserAccount(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new DeleteCurrentUserAccountCommand(userId), cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Khôi phục tài khoản người dùng
    /// </summary>
    [HttpPost("{userId:guid}/restore")]
    [ProducesResponseType(typeof(RestoreUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreUserAccount(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RestoreUserAccountCommand(userId), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
