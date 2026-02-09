using Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.LoginForManager;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Register;
using Application.Interfaces.Services;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class Auth
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IHttpTokenAccessorService> _httpTokenAccessorServiceMock;
    private readonly AuthController _controller;

    public Auth()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpTokenAccessorServiceMock = new Mock<IHttpTokenAccessorService>();
        _controller = new AuthController(_mediatorMock.Object, _httpTokenAccessorServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "AUTH_REG_002 - Đăng ký thất bại (Validation) - TH1: Thiếu Password")]
    public async Task AUTH_REG_002_1_Register_MissingPassword()
    {
        var request = new RegisterCommand
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = string.Empty,
            FullName = "Test User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed: Password is required"));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.RegisterAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "AUTH_REG_002 - Đăng ký thất bại (Validation) - TH2: Thiếu Email và Username")]
    public async Task AUTH_REG_002_2_Register_MissingEmailAndUsername()
    {
        var request = new RegisterCommand
        {
            Email = string.Empty,
            Username = string.Empty,
            Password = "Password123!",
            FullName = "Test User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed: Email and Username are required"));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.RegisterAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "AUTH_REG_002 - Đăng ký thất bại (Validation) - TH3: Thiếu FullName")]
    public async Task AUTH_REG_002_3_Register_MissingFullName()
    {
        var request = new RegisterCommand
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Password123!",
            FullName = string.Empty
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed: FullName is required"));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.RegisterAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "AUTH_LOG_002 - Đăng nhập sai thông tin")]
    public async Task AUTH_LOG_002_Login_Fail_WrongCreds()
    {
        var request = new LoginCommand { UsernameOrEmail = "user", Password = "wrong" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.LoginAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "AUTH_MGR_002 - Manager Login Fail (Quyền)")]
    public async Task AUTH_MGR_002_Login_Manager_Fail_Forbidden()
    {
        var request = new LoginForManagerCommand { UsernameOrEmail = "staff", Password = "123" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginForManagerCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Forbidden"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.LoginForManagerAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "AUTH_OUT_001 - Đăng xuất")]
    public async Task AUTH_OUT_001_Logout_Success()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.LogoutAsync(CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
