using Asp.Versioning;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using Infrastructure.Authorization;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.ApiContracts.Auth.Responses;

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
    [AnonymousOnly]
    [ProducesResponseType(typeof(RegistrationSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] Application.ApiContracts.Auth.Requests.RegisterRequest model)
    {
        if (!GenderStatus.IsValid(model.Gender))
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Gender not vaild. Please check again.", Field = "gender" }] });
        }

        var existingUser = await userManager.FindByNameAsync(model.Username);
        if (existingUser is not null)
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Username already exists.", Field = "username" }] });
        }

        existingUser = await userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            return BadRequest(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Email already exists.", Field = "email" }] });
        }

        var user = new ApplicationUser
        {
            UserName = string.IsNullOrEmpty(model.Username) ? model.Email : model.Username,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Gender = model.Gender ?? GenderStatus.Male,
            Status = UserStatus.Active
        };

        var result = await userManager.CreateAsync(user, model.Password);

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

        var defaultRoles = configuration.GetSection("ProtectedAuthorizationEntities:DefaultRolesForNewUsers").Get<List<string>>() ?? [];
        if (defaultRoles.Count > 0)
        {
            var randomRole = defaultRoles[Random.Shared.Next(defaultRoles.Count)];
            await userManager.AddToRoleAsync(user, randomRole);
        }

        return Ok(new RegistrationSuccessResponse());
    }

    /// <summary>
    /// Đăng nhập bằng Username/Email và Password
    /// </summary>
    [HttpPost("login")]
    [AnonymousOnly]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] Application.ApiContracts.Auth.Requests.LoginRequest model)
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
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid credentials." }] });
        }

        // Kiểm tra trạng thái của user
        if (user.Status != UserStatus.Active || user.DeletedAt is not null)
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Account is not available." }] });
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid credentials." }] });
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
    [ProducesResponseType(typeof(GetAccessTokenFromRefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Refresh token is missing." }] });
        }

        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user is null)
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Invalid refresh token." }] });
        }

        if (user.Status != "Active")
        {
            return StatusCode(StatusCodes.Status403Forbidden, (new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Please login again." }] }));
        }

        if (user.Status == "Active" && user.DeletedAt != null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, (new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Please login again." }] }));
        }

        if (user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Refresh token has expired. Please login again." }] });
        }

        // Validate status against stored JWT claim if authorization header exists
        var authHeader = Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..];
            try
            {
                var jwtKey = configuration["Jwt:Key"];
                if (!string.IsNullOrEmpty(jwtKey))
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(jwtKey);

                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    if (validatedToken is JwtSecurityToken jwtToken)
                    {
                        var tokenStatusClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "status")?.Value;
                        if (!string.IsNullOrEmpty(tokenStatusClaim) && tokenStatusClaim != user.Status)
                        {
                            return Unauthorized(new ErrorResponse() { Errors = [new ErrorDetail() { Message = "User status has changed. Please login again." }] });
                        }
                    }
                }
            }
            catch
            {
                // If token validation fails, continue with refresh - the token is just for status check
            }
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

        return Ok(new GetAccessTokenFromRefreshTokenResponse
        {
            AccessToken = newAccessToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        });
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutSuccessResponse), StatusCodes.Status200OK)]
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
        return Ok(new LogoutSuccessResponse());
    }

    /// <summary>
    /// Đăng nhập bằng Google (placeholder - cần cấu hình Google OAuth)
    /// </summary>
    [HttpPost("google")]
    [AnonymousOnly]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GoogleLogin([FromBody] Application.ApiContracts.Auth.Requests.GoogleLoginRequest model)
    {
        // TODO: Implement Google OAuth login
        // 1. Verify ID Token with Google
        // 2. Get user info from token
        // 3. Create or find user in database
        // 4. Generate JWT token
        return StatusCode(501, new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Google login not implemented yet." }] });
    }

    /// <summary>
    /// Đăng nhập bằng Facebook (placeholder - cần cấu hình Facebook OAuth)
    /// </summary>
    [HttpPost("facebook")]
    [AnonymousOnly]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> FacebookLogin([FromBody] Application.ApiContracts.Auth.Requests.FacebookLoginRequest model)
    {
        // TODO: Implement Facebook OAuth login
        // 1. Verify Access Token with Facebook
        // 2. Get user info from Facebook Graph API
        // 3. Create or find user in database
        // 4. Generate JWT token
        return StatusCode(501, new ErrorResponse() { Errors = [new ErrorDetail() { Message = "Facebook login not implemented yet." }] });
    }
}
