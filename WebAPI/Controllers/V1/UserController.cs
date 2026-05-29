using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Features.Users.Commands.ChangePassword;
using Application.Features.Users.Commands.DeleteCurrentUserAccount;
using Application.Features.Users.Commands.RestoreUserAccount;
using Application.Features.Users.Commands.UpdateCurrentUser;
using Application.Features.Users.Commands.UploadAvatarCurrentUser;
using Application.Features.Users.Queries.GetCurrentUser;
using Application.Features.Users.Queries.GetGenderOptions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class UserController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy thông tin người dùng hiện tại từ JWT (Hỗ trợ SSE nếu Accept: text/event-stream)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        bool isSse = Request.Headers.Accept.ToString().Contains("text/event-stream");
        if (isSse)
        {
            var stream = await mediator.Send(new GetCurrentUserStreamQuery(), cancellationToken).ConfigureAwait(false);
            return HandleSseResult(stream);
        } else
        {
            var result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken).ConfigureAwait(false);
            return HandleResult(result);
        }
    }

    /// <summary>
    /// Đổi thông tin người dùng hiện tại từ JWT
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDTOForManagerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrentUserAsync(
        [FromBody] UpdateCurrentUserCommand model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(model, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Đổi mật khẩu người dùng hiện tại từ JWT
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordByUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePasswordCurrentUserAsync(
        [FromBody] ChangePasswordCommand model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(model, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa tài khoản của người dùng từ JWT
    /// </summary>
    [HttpPost("delete-account")]
    [Authorize]
    [ProducesResponseType(typeof(DeleteAccountByUserReponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCurrentUserAccountAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCurrentUserAccountCommand(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục tài khoản người dùng
    /// </summary>
    [HttpPost("{userId:guid}/restore")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RestoreUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreUserAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RestoreUserAccountCommand() { UserId = userId }, cancellationToken)
            .ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Tải lên ảnh đại diện cho người dùng hiện tại
    /// </summary>
    [HttpPost("avatar")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadAvatarAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var command = new UploadAvatarCurrentUserCommand
        {
            FileContent = file.OpenReadStream(),
            FileName = file.FileName
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách ánh xạ giới tính (key tiếng Anh - nhãn tiếng Việt) để Frontend binding.
    /// </summary>
    [HttpGet("gender-options")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<GenderOptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGenderOptionsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetGenderOptionsQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
