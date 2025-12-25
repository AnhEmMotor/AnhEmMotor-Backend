using Application.ApiContracts.Auth.Responses;
using Application.Features.Auth.Commands.GoogleLogin;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller xử lý xác thực và đăng nhập
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Controller xử lý xác thực và đăng nhập")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
[ApiController]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("register")]
    [AnonymousOnly]
    [SwaggerOperation(Summary = "Đăng ký tài khoản mới", Description = "Tạo 1 tài khoản mới (với email và password)")]
    [ProducesResponseType(typeof(RegistrationSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] Application.ApiContracts.Auth.Requests.RegisterRequest model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterCommand(
                model.Username,
                model.Email,
                model.Password,
                model.FullName,
                model.PhoneNumber,
                model.Gender),
            cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Đăng nhập bằng Username/Email và Password
    /// </summary>
    [HttpPost("login")]
    [AnonymousOnly]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] Application.ApiContracts.Auth.Requests.LoginRequest model,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(model.UsernameOrEmail, model.Password), cancellationToken)
            .ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Làm mới Access Token bằng Refresh Token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetAccessTokenFromRefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RefreshTokenCommand(), cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutSuccessResponse), StatusCodes.Status200OK)]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await mediator.Send(new LogoutCommand(userId), cancellationToken).ConfigureAwait(true);

        Response.Cookies.Delete("refreshToken");
        return Ok(new LogoutSuccessResponse());
    }

    /// <summary>
    /// Đăng nhập bằng Google (placeholder - cần cấu hình Google OAuth)
    /// </summary>
    [HttpPost("google")]
    [AnonymousOnly]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] Application.ApiContracts.Auth.Requests.GoogleLoginRequest model,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new GoogleLoginCommand(model), cancellationToken).ConfigureAwait(true);
        return Ok();
    }

    /// <summary>
    /// Đăng nhập bằng Facebook (placeholder - cần cấu hình Facebook OAuth)
    /// </summary>
    [HttpPost("facebook")]
    [AnonymousOnly]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status501NotImplemented)]
    public IActionResult FacebookLogin()
    {
        // TODO: Implement Facebook OAuth login
        return StatusCode(
            501,
            new Application.Common.Models.ErrorResponse()
            {
                Errors =
                    [ new Application.Common.Models.ErrorDetail() { Message = "Facebook login not implemented yet." } ]
            });
    }
}
