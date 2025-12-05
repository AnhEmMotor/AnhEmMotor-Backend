using Application.ApiContracts.Auth;
using Asp.Versioning;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller xử lý xác thực và đăng nhập
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Controller xử lý xác thực và đăng nhập")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
[ApiController]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    TokenService tokenService,
    IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        // Kiểm tra username đã tồn tại
        var existingUser = await userManager.FindByNameAsync(model.Username);
        if (existingUser is not null)
        {
            return BadRequest(new { Message = "Username already exists." });
        }

        // Kiểm tra email đã tồn tại
        existingUser = await userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            return BadRequest(new { Message = "Email already exists." });
        }

        var user = new ApplicationUser
        {
            UserName = string.IsNullOrEmpty(model.Username) == true ? model.Email : model.Username,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Gender = model.Gender ?? GenderStatus.Male,
            Status = UserStatus.Active
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { result.Errors });
        }

        // Gán default roles cho user mới
        var defaultRoles = configuration.GetSection("ProtectedAuthorizationEntities:DefaultRolesForNewUsers").Get<List<string>>() ?? [];
        if (defaultRoles.Count > 0)
        {
            var randomRole = defaultRoles[Random.Shared.Next(defaultRoles.Count)];
            await userManager.AddToRoleAsync(user, randomRole);
        }

        return Ok(new { Message = "Registration successful!" });
    }

    /// <summary>
    /// Đăng nhập bằng Username/Email và Password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        ApplicationUser? user;

        // Kiểm tra xem người dùng nhập username hay email
        if (model.UsernameOrEmail.Contains('@'))
        {
            user = await userManager.FindByEmailAsync(model.UsernameOrEmail);
        }
        else
        {
            user = await userManager.FindByNameAsync(model.UsernameOrEmail);
        }

        if (user is null)
        {
            return Unauthorized(new { Message = "Invalid credentials." });
        }

        // Kiểm tra trạng thái của user
        if (user.Status != UserStatus.Active || user.DeletedAt is not null)
        {
            return Unauthorized(new { Message = "Account is not available." });
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { Message = "Invalid credentials." });
        }

        var accessToken = await tokenService.CreateAccessTokenAsync(user, ["pwd"]);
        var refreshToken = TokenService.CreateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        });
    }

    /// <summary>
    /// Làm mới Access Token bằng Refresh Token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { Message = "Refresh token is missing." });
        }

        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user is null)
        {
            return Unauthorized(new { Message = "Invalid refresh token." });
        }

        if (user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            return Unauthorized(new { Message = "Refresh token has expired. Please login again." });
        }

        var newAccessToken = await tokenService.CreateAccessTokenAsync(user, ["pwd"]);
        var newRefreshToken = TokenService.CreateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

        return Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        });
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is not null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTimeOffset.MinValue;
                await userManager.UpdateAsync(user);
            }
        }

        Response.Cookies.Delete("refreshToken");
        return Ok(new { Message = "Logged out successfully." });
    }

    /// <summary>
    /// Đăng nhập bằng Google (placeholder - cần cấu hình Google OAuth)
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest model)
    {
        // TODO: Implement Google OAuth login
        // 1. Verify ID Token with Google
        // 2. Get user info from token
        // 3. Create or find user in database
        // 4. Generate JWT token
        return StatusCode(501, new { Message = "Google login not implemented yet." });
    }

    /// <summary>
    /// Đăng nhập bằng Facebook (placeholder - cần cấu hình Facebook OAuth)
    /// </summary>
    [HttpPost("facebook")]
    [AllowAnonymous]
    public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequest model)
    {
        // TODO: Implement Facebook OAuth login
        // 1. Verify Access Token with Facebook
        // 2. Get user info from Facebook Graph API
        // 3. Create or find user in database
        // 4. Generate JWT token
        return StatusCode(501, new { Message = "Facebook login not implemented yet." });
    }
}
