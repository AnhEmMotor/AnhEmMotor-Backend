using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Features.Auth.Commands.FacebookLogin;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.Commands.GoogleLogin;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.LoginForManager;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.ResetPassword;
using Application.Features.Auth.Queries.GetExternalAuthConfig;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller xử lý xác thực và đăng nhập
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Controller xử lý xác thực và đăng nhập")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class AuthController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Đăng ký tài khoản mới với email và mật khẩu.
    /// </summary>
    /// <param name="command">Thông tin đăng ký (email, password, họ tên, v.v.).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả đăng ký — thông tin tài khoản mới tạo.</returns>
    /// <response code="200">Đăng ký thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc email đã tồn tại.</response>
    [HttpPost("register")]
    [AnonymousOnly]
    [EnableRateLimiting("public_api")]
    [SwaggerOperation(Summary = "Đăng ký tài khoản mới", Description = "Tạo 1 tài khoản mới (với email và password)")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleCreated(result);
    }

    /// <summary>
    /// Đăng nhập bằng tên đăng nhập (username/email) và mật khẩu.
    /// </summary>
    /// <param name="command">Thông tin đăng nhập (username/email, password).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả đăng nhập — access token, refresh token, thông tin người dùng.</returns>
    /// <response code="200">Đăng nhập thành công.</response>
    /// <response code="401">Thông tin đăng nhập không chính xác.</response>
    [HttpPost("login")]
    [EnableRateLimiting("public_api")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Làm mới Access Token bằng Refresh Token hợp lệ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Access token mới và thông tin refresh token.</returns>
    /// <response code="200">Làm mới token thành công.</response>
    /// <response code="401">Refresh token không hợp lệ hoặc đã hết hạn.</response>
    /// <response code="403">Refresh token bị thu hồi.</response>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting("public_api")]
    [ProducesResponseType(typeof(GetAccessTokenFromRefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RefreshTokenCommand(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Đăng xuất tài khoản hiện tại — vô hiệu hóa refresh token.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả đăng xuất.</returns>
    /// <response code="200">Đăng xuất thành công.</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    [Authorize]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LogoutCommand(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Đăng nhập bằng tài khoản Google (Social Login).
    /// </summary>
    /// <param name="command">Token xác thực từ Google.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả đăng nhập — access token, refresh token, thông tin người dùng.</returns>
    /// <response code="200">Đăng nhập Google thành công.</response>
    /// <response code="401">Token Google không hợp lệ.</response>
    [HttpPost("google")]
    [EnableRateLimiting("public_api")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLoginAsync(
        [FromBody] GoogleLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Đăng nhập bằng tài khoản Facebook (Social Login).
    /// </summary>
    /// <param name="command">Token xác thực từ Facebook.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả đăng nhập — access token, refresh token, thông tin người dùng.</returns>
    /// <response code="200">Đăng nhập Facebook thành công.</response>
    /// <response code="401">Token Facebook không hợp lệ.</response>
    [HttpPost("facebook")]
    [EnableRateLimiting("public_api")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FacebookLoginAsync(
        [FromBody] FacebookLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Đăng nhập dành riêng cho quản lý — yêu cầu người dùng có ít nhất một quyền trong hệ thống.
    /// </summary>
    /// <param name="command">Thông tin đăng nhập (username/email, password).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả đăng nhập — access token, refresh token, thông tin người dùng với danh sách quyền.</returns>
    /// <response code="200">Đăng nhập quản lý thành công.</response>
    /// <response code="401">Thông tin đăng nhập không chính xác.</response>
    /// <response code="403">Người dùng không có quyền truy cập hệ thống quản lý.</response>
    [HttpPost("login/for-manager")]
    [EnableRateLimiting("public_api")]
    [SwaggerOperation(
        Summary = "Đăng nhập cho quản lý",
        Description = "Đăng nhập với Username/Email và Password. Chỉ cho phép người dùng có ít nhất một quyền trong hệ thống.")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LoginForManagerAsync(
        [FromBody] LoginForManagerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Gửi email chứa liên kết đặt lại mật khẩu cho người dùng.
    /// </summary>
    /// <param name="command">Địa chỉ email của tài khoản cần đặt lại mật khẩu.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả yêu cầu đặt lại mật khẩu.</returns>
    /// <response code="200">Email đặt lại mật khẩu đã được gửi (nếu email tồn tại).</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("public_api")]
    [SwaggerOperation(Summary = "Quên mật khẩu", Description = "Gửi email chứa liên kết đặt lại mật khẩu")]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Đặt lại mật khẩu bằng token nhận được qua email.
    /// </summary>
    /// <param name="command">Token đặt lại mật khẩu và mật khẩu mới.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả đặt lại mật khẩu.</returns>
    /// <response code="200">Đặt lại mật khẩu thành công.</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("public_api")]
    [SwaggerOperation(Summary = "Đặt lại mật khẩu", Description = "Đặt mật khẩu mới bằng token từ email")]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy cấu hình các dịch vụ xác thực bên ngoài (Social Login).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Cấu hình Social Login — Google Client ID và Facebook App ID.</returns>
    /// <response code="200">Trả về cấu hình thành công.</response>
    [HttpGet("external-config")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Lấy cấu hình Social Login", Description = "Lấy Google Client ID và Facebook App ID")]
    [ProducesResponseType(typeof(ExternalAuthConfigResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExternalAuthConfigAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetExternalAuthConfigQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
