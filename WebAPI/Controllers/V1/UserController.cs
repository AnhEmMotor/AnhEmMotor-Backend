using Application.ApiContracts.User.Responses;
using Application.Features.UserManager.Commands.ChangePassword;
using Application.Features.UserManager.Commands.UpdateUser;
using Application.Features.Users.Commands.DeleteCurrentUserAccount;
using Application.Features.Users.Commands.RestoreUserAccount;

using Application.Features.Users.Queries.GetCurrentUser;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
public class UserController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy thông tin người dùng hiện tại từ JWT
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new GetCurrentUserQuery() { UserId = userId }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
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
    public async Task<IActionResult> UpdateCurrentUserAsync(
        [FromBody] UpdateUserCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
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
    public async Task<IActionResult> ChangePasswordCurrentUserAsync(
        [FromBody] ChangePasswordCommand model,
        CancellationToken cancellationToken)
    {
        var modelToSend = model with { UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
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
    public async Task<IActionResult> DeleteCurrentUserAccountAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new DeleteCurrentUserAccountCommand() { UserId = userId }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục tài khoản người dùng
    /// </summary>
    [HttpPost("{userId:guid}/restore")]
    [ProducesResponseType(typeof(RestoreUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreUserAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RestoreUserAccountCommand() { UserId = userId }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }
}
