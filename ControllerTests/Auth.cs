using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Register;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;
using Xunit;

namespace ControllerTests;

public class Auth
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;

    public Auth()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController(_mediatorMock.Object);
        
        // Setup Controller Context if needed (e.g. for Cookies)
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "AUTH_REG_002 - Đăng ký thất bại (Validation) - TH1: Thiếu Password")]
    public async Task AUTH_REG_002_1_Register_MissingPassword()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "", // Missing
            FullName = "Test User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed: Password is required"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _controller.Register(request, CancellationToken.None));
    }

    [Fact(DisplayName = "AUTH_REG_002 - Đăng ký thất bại (Validation) - TH2: Thiếu Email và Username")]
    public async Task AUTH_REG_002_2_Register_MissingEmailAndUsername()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "", // Missing
            Username = "", // Missing
            Password = "Password123!",
            FullName = "Test User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed: Email and Username are required"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _controller.Register(request, CancellationToken.None));
    }

    [Fact(DisplayName = "AUTH_REG_002 - Đăng ký thất bại (Validation) - TH3: Thiếu FullName")]
    public async Task AUTH_REG_002_3_Register_MissingFullName()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Password123!",
            FullName = "" // Missing
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed: FullName is required"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _controller.Register(request, CancellationToken.None));
    }

    [Fact(DisplayName = "AUTH_LOG_002 - Đăng nhập sai thông tin")]
    public async Task AUTH_LOG_002_Login_Fail_WrongCreds()
    {
        // Arrange
        var request = new LoginRequest { UsernameOrEmail = "user", Password = "wrong" };
        
        // Mock Mediator to return Failure
        // Assuming Handler returns Result<LoginResponse>
        // I'll mock it to throw or return error.
        // Let's assume it returns a Result object that the Controller maps to BadRequest.
        // Since I can't see the Controller implementation (it threw NotImplementedException in the provided file),
        // I will assume the Controller delegates to Mediator.
        
        // I'll mock the Mediator to return a specific error response if possible, or just verify the call.
        // But to test "Fail", I need to simulate the failure.
        
        // Let's assume the handler throws.
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Login(request, CancellationToken.None));
    }

    [Fact(DisplayName = "AUTH_MGR_002 - Manager Login Fail (Quyền)")]
    public async Task AUTH_MGR_002_Login_Manager_Fail_Forbidden()
    {
        // Arrange
        var request = new LoginRequest { UsernameOrEmail = "staff", Password = "123" };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Forbidden"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.LoginForManager(request, CancellationToken.None));
    }

    [Fact(DisplayName = "AUTH_OUT_001 - Đăng xuất")]
    public async Task AUTH_OUT_001_Logout_Success()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Logout(CancellationToken.None);

        // Assert
        // Assuming Controller returns Ok(response)
        result.Should().BeOfType<OkObjectResult>();
        // Since implementation is missing, I'll just verify the call.
        _mediatorMock.Verify(m => m.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
