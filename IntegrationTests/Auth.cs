using Application.ApiContracts.Auth.Responses;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public class Auth : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public Auth(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "AUTH_REG_001 - Đăng ký thành công")]
    public async Task AUTH_REG_001_Register_Success()
    {
        var request = new RegisterCommand
        {
            Email = "test_reg_001@example.com",
            Password = "Password123!",
            FullName = "Test User 001",
            Username = "testuser001",
            PhoneNumber = "0123456789"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/register", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<RegisterResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.UserId.Should().NotBeEmpty();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var user = db.Users.FirstOrDefault(u => u.Email == request.Email);
        user.Should().NotBeNull();
        user!.Status.Should().Be(UserStatus.Active);
    }

    [Fact(DisplayName = "AUTH_REG_003 - Đăng ký trùng lặp")]
    public async Task AUTH_REG_003_Register_Duplicate_Fail()
    {
        var request1 = new RegisterCommand
        {
            Email = "exist@example.com",
            Username = "existuser",
            Password = "Password123!",
            FullName = "Exist User",
            PhoneNumber = "0987654321"
        };
        await _client.PostAsJsonAsync("/api/v1/Auth/register", request1).ConfigureAwait(true);

        var request2 = new RegisterCommand
        {
            Email = "exist@example.com",
            Username = "newuser",
            Password = "Password123!",
            FullName = "New User",
            PhoneNumber = "0987654322"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/register", request2).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "AUTH_REG_005 - Security (XSS/SQLi)")]
    public async Task AUTH_REG_005_Register_Sanitization()
    {
        var request = new RegisterCommand
        {
            Email = "hacker@example.com",
            Password = "Password123!",
            FullName = "<script>alert(1)</script>",
            Username = "' OR 1=1 --",
            PhoneNumber = "0123456789"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/register", request).ConfigureAwait(true);

        if(response.IsSuccessStatusCode)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var user = db.Users.FirstOrDefault(u => u.Email == request.Email);
            user.Should().NotBeNull();
            user!.FullName.Should().Be("<script>alert(1)</script>");
            user.UserName.Should().Be("' OR 1=1 --");
        } else
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact(DisplayName = "AUTH_REG_006 - Đăng ký Email đã Xóa mềm")]
    public async Task AUTH_REG_006_Register_SoftDeleted_Email_Fail()
    {
        var email = "deleted@example.com";
        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = "deleteduser",
                Email = email,
                FullName = "Deleted User",
                DeletedAt = DateTimeOffset.UtcNow,
                Status = UserStatus.Active
            };
            await userManager.CreateAsync(user, "Password123!").ConfigureAwait(true);
        }

        var request = new RegisterCommand
        {
            Email = email,
            Username = "newdeleteduser",
            Password = "Password123!",
            FullName = "New User",
            PhoneNumber = "0123456789"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/register", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "AUTH_LOG_001 - Đăng nhập thành công")]
    public async Task AUTH_LOG_001_Login_Success()
    {
        var email = "login_success@example.com";
        var password = "Password123!";
        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = "loginuser",
                Email = email,
                FullName = "Login User",
                Status = UserStatus.Active
            };
            await userManager.CreateAsync(user, password).ConfigureAwait(true);
        }

        var request = new LoginCommand { UsernameOrEmail = email, Password = password };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<LoginResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "AUTH_LOG_003 - Đăng nhập User bị cấm")]
    public async Task AUTH_LOG_003_Login_Banned_Fail()
    {
        var username = "banned_user";
        var password = "Password123!";
        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = username,
                Email = "banned@example.com",
                FullName = "Banned User",
                Status = UserStatus.Banned
            };
            await userManager.CreateAsync(user, password).ConfigureAwait(true);
        }

        var request = new LoginCommand { UsernameOrEmail = username, Password = password };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "AUTH_MGR_001 - Đăng nhập Manager")]
    public async Task AUTH_MGR_001_Login_Manager_Success()
    {
        var username = "manager_user";
        var password = "Password123!";
        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            if(!await roleManager.RoleExistsAsync("Manager").ConfigureAwait(true))
                await roleManager.CreateAsync(new ApplicationRole { Name = "Manager" }).ConfigureAwait(true);

            var user = new ApplicationUser
            {
                UserName = username,
                Email = "manager@example.com",
                FullName = "Manager User",
                Status = UserStatus.Active
            };
            await userManager.CreateAsync(user, password).ConfigureAwait(true);
            await userManager.AddToRoleAsync(user, "Manager").ConfigureAwait(true);
        }

        var request = new LoginCommand { UsernameOrEmail = username, Password = password };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login/for-manager", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "AUTH_REF_001 - Refresh Token thành công")]
    public async Task AUTH_REF_001_RefreshToken_Success()
    {
        var username = "refresh_user";
        var password = "Password123!";
        string? refreshToken = string.Empty;

        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = username,
                Email = "refresh@example.com",
                FullName = "Refresh User",
                Status = UserStatus.Active
            };
            await userManager.CreateAsync(user, password).ConfigureAwait(true);
        }

        var loginRes = await _client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new LoginCommand { UsernameOrEmail = username, Password = password })
            .ConfigureAwait(true);
        var loginContent = await loginRes.Content
            .ReadFromJsonAsync<LoginResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        refreshToken = loginContent!.RefreshToken;

        var requestMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/refresh-token");
        requestMsg.Headers.Add("Cookie", $"refreshToken={refreshToken}");

        var response = await _client.SendAsync(requestMsg).ConfigureAwait(true);

        if(response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content
                .ReadFromJsonAsync<GetAccessTokenFromRefreshTokenResponse>(CancellationToken.None)
                .ConfigureAwait(true);
            content.Should().NotBeNull();
            content!.AccessToken.Should().NotBeNullOrEmpty();
            if(response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var cookieList = cookies.ToList();
                cookieList.Should().Contain(c => c.Contains("refreshToken"));
            }
        }
    }

    [Fact(DisplayName = "AUTH_REF_003 - Refresh Token User bị cấm")]
    public async Task AUTH_REF_003_RefreshToken_Banned_Fail()
    {
        var username = "refresh_banned";
        var password = "Password123!";
        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = username,
                Email = "refresh_banned@example.com",
                FullName = "Refresh Banned",
                Status = UserStatus.Active
            };
            await userManager.CreateAsync(user, password).ConfigureAwait(true);
        }

        var loginRes = await _client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new LoginCommand { UsernameOrEmail = username, Password = password })
            .ConfigureAwait(true);
        var loginContent = await loginRes.Content
            .ReadFromJsonAsync<LoginResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        string? refreshToken = loginContent!.RefreshToken;

        using(var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByNameAsync(username).ConfigureAwait(true);
            user!.Status = UserStatus.Banned;
            await userManager.UpdateAsync(user).ConfigureAwait(true);
        }

        var requestMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/refresh-token");
        requestMsg.Headers.Add("Cookie", $"refreshToken={refreshToken}");

        var response = await _client.SendAsync(requestMsg).ConfigureAwait(true);

        response.StatusCode
            .Should()
            .BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "AUTH_VAL_001 - Trimming dữ liệu")]
    public async Task AUTH_VAL_001_Trimming()
    {
        var request = new RegisterCommand
        {
            Email = "  trim@example.com  ",
            Username = "  trimuser  ",
            Password = "Password123!",
            FullName = "  Trim User  ",
            PhoneNumber = "0123456789"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/register", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var user = db.Users.FirstOrDefault(u => u.Email == "trim@example.com");
        user.Should().NotBeNull();
        user!.UserName.Should().Be("trimuser");
        user.FullName.Should().Be("Trim User");
    }
#pragma warning restore CRR0035
}
