using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Features.Auth.Commands.FacebookLogin;
using Application.Features.Auth.Commands.GoogleLogin;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.LoginForManager;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Queries.GetExternalAuthConfig;
using Application.Interfaces.Services;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller xử lý xác thực và đăng nhập
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Controller xử lý xác thực và đăng nhập")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class AuthController(IMediator mediator, IHttpTokenAccessorService httpTokenAccessorService) : ApiController
{
    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
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
    /// Đăng nhập bằng Username/Email và Password
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("public_api")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        return HandleResult(result);
    }

    /// <summary>
    /// Làm mới Access Token bằng Refresh Token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting("public_api")]
    [ProducesResponseType(typeof(GetAccessTokenFromRefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        var refreshToken = httpTokenAccessorService.GetRefreshTokenFromCookie();
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        var result = await mediator.Send(
            new RefreshTokenCommand() { RefreshToken = refreshToken, AccessToken = accessToken },
            cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    [Authorize]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await mediator.Send(new LogoutCommand() { UserId = userId }, cancellationToken)
            .ConfigureAwait(true);
        if(result.IsFailure)
            return HandleResult(result);

        httpTokenAccessorService.DeleteRefreshTokenFromCookie();
        return Ok(new LogoutResponse());
    }

    /// <summary>
    /// Đăng nhập bằng Google
    /// </summary>
    [HttpPost("google")]
    [EnableRateLimiting("public_api")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLoginAsync(
        [FromBody] GoogleLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Đăng nhập bằng Facebook
    /// </summary>
    [HttpPost("facebook")]
    [EnableRateLimiting("public_api")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FacebookLoginAsync(
        [FromBody] FacebookLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Đăng nhập bằng Username/Email và Password - Dành cho quản lý
    /// </summary>
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
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        return HandleResult(result);
    }

    /// <summary>
    /// Lấy cấu hình các dịch vụ xác thực bên ngoài
    /// </summary>
    [HttpGet("external-config")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Lấy cấu hình Social Login", Description = "Lấy Google Client ID và Facebook App ID")]
    [ProducesResponseType(typeof(ExternalAuthConfigResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExternalAuthConfigAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetExternalAuthConfigQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
