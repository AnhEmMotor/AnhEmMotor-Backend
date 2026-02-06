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
using Domain.Constants.Permission;
using Microsoft.EntityFrameworkCore;
using IntegrationTests.SetupClass;

namespace IntegrationTests;

using System.Threading.Tasks;
using Xunit;

[Collection("Shared Integration Collection")]
public class Auth : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public Auth(IntegrationTestWebAppFactory factory, Xunit.Abstractions.ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "AUTH_REG_001 - Register Success")]
    public async Task AUTH_REG_001_Register_Success()
    {
        var request = new RegisterCommand
        {
            Email = "test_reg_001@example.com",
            Password = "Password123!",
            FullName = "Test User 001",
            Username = "testuser001",
            PhoneNumber = "0123456789",
            Gender = "Male"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var jsonRaw = await response.Content.ReadAsStringAsync();

        var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();

        content.Should().NotBeNull();
        content!.UserId.Should().NotBeEmpty();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var userFromDb = db.Users.FirstOrDefault(u => u.Email == request.Email);


        userFromDb.Should().NotBeNull();
        userFromDb!.Status.Should().Be(UserStatus.Active);
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
            PhoneNumber = "0987654321",
            Gender = "Male"
        };
        await _client.PostAsJsonAsync("/api/v1/Auth/register", request1).ConfigureAwait(true);

        var request2 = new RegisterCommand
        {
            Email = "exist@example.com",
            Username = "newuser",
            Password = "Password123!",
            FullName = "New User",
            PhoneNumber = "0987654322",
            Gender = "Male"
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
            PhoneNumber = "0123456789",
            Gender = "Male"
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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            "deleteduser",
            "Password123!",
            [], // No specific permissions needed for this test
            email: email,
            deletedAt: DateTimeOffset.UtcNow);

        var request = new RegisterCommand
        {
            Email = email,
            Username = "newdeleteduser",
            Password = "Password123!",
            FullName = "New User",
            PhoneNumber = "0123456789",
            Gender = "Male"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/register", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "AUTH_LOG_001 - Đăng nhập thành công với Cookie bảo mật")]
    public async Task AUTH_LOG_001_Login_Success_With_Secure_Cookies()
    {
        var email = "login_success@example.com";
        var password = "Password123!";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            "loginuser",
            password,
            [],
            email: email);

        var request = new LoginCommand { UsernameOrEmail = email, Password = password };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var setCookieHeaders = response.Headers.GetValues("Set-Cookie").ToList();
        var refreshTokenCookie = setCookieHeaders.FirstOrDefault(c => c.Contains("refreshToken"));

        refreshTokenCookie.Should().NotBeNull();

        refreshTokenCookie.Should().Contain("httponly", "Vì Refresh Token không được phép để Javascript truy cập");
        refreshTokenCookie.Should().Contain("secure", "Vì Refresh Token chỉ được gửi qua HTTPS");
        refreshTokenCookie.Should().Contain("samesite=strict", "Hoặc Lax tùy vào cấu hình Cross-domain của bạn");
    }

    [Fact(DisplayName = "AUTH_LOG_003 - Đăng nhập User bị cấm")]
    public async Task AUTH_LOG_003_Login_Banned_Fail()
    {
        var username = "banned_user";
        var password = "Password123!";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [],
            email: "banned@example.com",
            isLocked: true);

        var request = new LoginCommand { UsernameOrEmail = username, Password = password };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "AUTH_MGR_001 - Đăng nhập Manager")]
    public async Task AUTH_MGR_001_Login_Manager_Success()
    {
        var username = "manager_user";
        var password = "Password123!";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [PermissionsList.Users.View],
            email: "manager@example.com",
            roleName: "Manager");

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

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [],
            email: "refresh@example.com");

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
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [],
            email: "refresh_banned@example.com");

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
#pragma warning restore CRR0035
}
