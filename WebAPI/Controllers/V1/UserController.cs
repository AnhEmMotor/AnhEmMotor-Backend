using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Features.Users.Commands.ChangePassword;
using Application.Features.Users.Commands.DeleteCurrentUserAccount;
using Application.Features.Users.Commands.RestoreUserAccount;
using Application.Features.Users.Commands.UpdateCurrentUser;
using Application.Features.Users.Commands.UploadAvatar;
using Application.Features.Users.Queries.GetCurrentUser;
using Application.Interfaces.Services;
using Asp.Versioning;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.Text.Json;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý người dùng (Bất cứ người dùng nào đã đăng nhập đều có quyền vào đây)")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class UserController(IMediator mediator, IUserStreamService userStreamService, IServiceProvider serviceProvider) : ApiController
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
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
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized(Error.Validation("Invalid User ID", "UserId"));
        }

        bool isSse = Request.Headers.Accept.ToString().Contains("text/event-stream");

        if(isSse)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("X-Accel-Buffering", "no");

            try
            {
                await SendUserDataAsync(mediator, userIdString, cancellationToken).ConfigureAwait(false);

                while(!cancellationToken.IsCancellationRequested)
                {
                    await userStreamService.WaitForUpdateAsync(userId, cancellationToken).ConfigureAwait(true);

                    using var scope = serviceProvider.CreateScope();
                    var scopedMediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await SendUserDataAsync(scopedMediator, userIdString, cancellationToken).ConfigureAwait(false);
                }
            } catch(OperationCanceledException)
            {
            }
            return new EmptyResult();
        } else
        {
            var result = await mediator.Send(new GetCurrentUserQuery() { UserId = userIdString }, cancellationToken)
                .ConfigureAwait(false);

            return HandleResult(result);
        }
    }

    private async Task SendUserDataAsync(IMediator mediatorToUse, string userId, CancellationToken cancellationToken)
    {
        var result = await mediatorToUse.Send(new GetCurrentUserQuery() { UserId = userId }, cancellationToken)
            .ConfigureAwait(false);

        if(result.IsSuccess)
        {
            var json = JsonSerializer.Serialize(result.Value, _jsonSerializerOptions);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken).ConfigureAwait(true);
            await Response.Body.FlushAsync(cancellationToken).ConfigureAwait(true);
        } else
        {
            var errorJson = JsonSerializer.Serialize(result.Error, _jsonSerializerOptions);
            await Response.WriteAsync($"event: error\ndata: {errorJson}\n\n", cancellationToken).ConfigureAwait(true);
            await Response.Body.FlushAsync(cancellationToken).ConfigureAwait(true);
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
        var modelToSend = model with { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(false);
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
        var modelToSend = model with { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) };
        var result = await mediator.Send(modelToSend, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá tài khoản của người dùng từ JWT
    /// </summary>
    [HttpPost("delete-account")]
    [Authorize]
    [ProducesResponseType(typeof(DeleteAccountByUserReponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
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
        ArgumentNullException.ThrowIfNull(file);
        var command = new UploadAvatarCommand
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
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
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public IActionResult GetGenderOptions()
    {
        var options = new[]
        {
            new { key = nameof(GenderStatus.Male), label = "Nam" },
            new { key = nameof(GenderStatus.Female), label = "Nữ" },
            new { key = nameof(GenderStatus.Other), label = "Khác" }
        };

        return Ok(options);
    }
}
