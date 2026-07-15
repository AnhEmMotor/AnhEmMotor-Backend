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
    /// Lấy thông tin người dùng hiện tại từ JWT (hỗ trợ SSE nếu header Accept: text/event-stream).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin người dùng hiện tại.</returns>
    /// <response code="200">Lấy thông tin thành công.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    /// <response code="404">Không tìm thấy người dùng.</response>
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
    /// Cập nhật thông tin cá nhân của người dùng hiện tại (họ tên, số điện thoại, địa chỉ, v.v.).
    /// </summary>
    /// <param name="model">Thông tin cập nhật của người dùng.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin người dùng sau khi cập nhật.</returns>
    /// <response code="200">Cập nhật thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    /// <response code="404">Không tìm thấy người dùng.</response>
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
    /// Đổi mật khẩu cho người dùng hiện tại (cần xác thực mật khẩu cũ).
    /// </summary>
    /// <param name="model">Thông tin đổi mật khẩu (mật khẩu cũ, mật khẩu mới).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả đổi mật khẩu.</returns>
    /// <response code="200">Đổi mật khẩu thành công.</response>
    /// <response code="400">Mật khẩu cũ không đúng hoặc mật khẩu mới không hợp lệ.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    /// <response code="404">Không tìm thấy người dùng.</response>
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
    /// Xóa tài khoản của người dùng hiện tại (yêu cầu xác thực mật khẩu).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xóa tài khoản.</returns>
    /// <response code="200">Xóa tài khoản thành công.</response>
    /// <response code="400">Không thể xóa tài khoản (có thể đang còn đơn hàng liên quan).</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    /// <response code="404">Không tìm thấy người dùng.</response>
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
    /// Khôi phục tài khoản người dùng đã bị xóa mềm (soft-delete) bằng token khôi phục.
    /// </summary>
    /// <param name="userId">ID (GUID) của người dùng cần khôi phục.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả khôi phục tài khoản.</returns>
    /// <response code="200">Khôi phục tài khoản thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc tài khoản đã được khôi phục.</response>
    /// <response code="404">Không tìm thấy người dùng hoặc token khôi phục không hợp lệ.</response>
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
    /// Tải lên ảnh đại diện (avatar) cho người dùng hiện tại (hỗ trợ file JPEG, PNG).
    /// </summary>
    /// <param name="file">Tệp hình ảnh cần tải lên.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>URL của ảnh đại diện đã tải lên.</returns>
    /// <response code="200">Tải lên ảnh đại diện thành công.</response>
    /// <response code="400">File rỗng hoặc định dạng không hợp lệ.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
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
