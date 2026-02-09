using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Features.Users.Commands.DeleteCurrentUserAccount;
using Application.Features.Users.Commands.RestoreUserAccount;
using Application.Features.Users.Commands.UpdateCurrentUser;
using Application.Features.Users.Queries.GetCurrentUser;
using Application.Interfaces.Services;
using Domain.Constants;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Security.Claims;

using WebAPI.Controllers.V1;

namespace ControllerTests;

public class User
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IUserStreamService> _userStreamServiceMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly UserController _controller;

    public User()
    {
        _mediatorMock = new Mock<IMediator>();
        _userStreamServiceMock = new Mock<IUserStreamService>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _controller = new UserController(
            _mediatorMock.Object,
            _userStreamServiceMock.Object,
            _serviceProviderMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "USER_036 - Controller - GET /api/v1/User/me gọi đúng Query")]
    public async Task GetCurrentUser_CallsCorrectQuery_ReturnsOk()
    {
        var expectedResponse = new UserResponse
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test User",
            Gender = GenderStatus.Male,
            PhoneNumber = "0123456789"
        };

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, expectedResponse.Id.ToString() ?? string.Empty) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(expectedResponse));

        var result = await _controller.GetCurrentUserAsync(CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "USER_037 - Controller - GET /api/v1/User/me xử lý exception")]
    public async Task GetCurrentUser_HandlesException_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid JWT"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetCurrentUserAsync(CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "USER_038 - Controller - PUT /api/v1/User/me gọi đúng Command")]
    public async Task UpdateCurrentUser_CallsCorrectCommand_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateCurrentUserCommand { FullName = "Test", Gender = GenderStatus.Male };

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var expectedResponse = new UserDTOForManagerResponse
        {
            Id = userId,
            FullName = "Test",
            Gender = GenderStatus.Male
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCurrentUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDTOForManagerResponse>.Success(expectedResponse));

        var result = await _controller.UpdateCurrentUserAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<UpdateCurrentUserCommand>(c => string.Compare(c.UserId, userId.ToString()) == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "USER_039 - Controller - PUT /api/v1/User/me với body null hoặc rỗng")]
    public async Task UpdateCurrentUser_EmptyBody_CallsMediator()
    {
        var request = new UpdateCurrentUserCommand();

        var expectedResponse = new UserDTOForManagerResponse { Id = Guid.NewGuid() };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCurrentUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.UpdateCurrentUserAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateCurrentUserCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "USER_040 - Controller - PUT /api/v1/User/me xử lý ValidationException")]
    public async Task UpdateCurrentUser_ValidationException_ThrowsValidationException()
    {
        var request = new UpdateCurrentUserCommand { PhoneNumber = "invalid" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCurrentUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDTOForManagerResponse>.Failure(Error.BadRequest("Invalid phone number format")));

        var result = await _controller.UpdateCurrentUserAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "USER_041 - Controller - POST /api/v1/User/change-password gọi đúng Command")]
    public async Task ChangePassword_CallsCorrectCommand_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var request = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand
        {
            CurrentPassword = "Old",
            NewPassword = "New"
        };
        var expectedResponse = new ChangePasswordByUserResponse { Message = "Password changed successfully" };

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.IsAny<Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChangePasswordByUserResponse>.Success(expectedResponse));

        var result = await _controller.ChangePasswordCurrentUserAsync(request, CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand>(
                    c => string.Compare(c.UserId, userId.ToString()) == 0 &&
                        string.Compare(c.NewPassword, "New") == 0 &&
                        string.Compare(c.CurrentPassword, "Old") == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "USER_042 - Controller - POST /api/v1/User/change-password với body thiếu trường")]
    public async Task ChangePassword_MissingField_ThrowsValidationException()
    {
        var request = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand
        {
            CurrentPassword = "Old",
            NewPassword = string.Empty
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.IsAny<Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChangePasswordByUserResponse>.Failure(Error.BadRequest("NewPassword is required")));

        var result = await _controller.ChangePasswordCurrentUserAsync(request, CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "USER_043 - Controller - POST /api/v1/User/change-password xử lý UnauthorizedException")]
    public async Task ChangePassword_UnauthorizedException_ThrowsUnauthorizedException()
    {
        var request = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand
        {
            CurrentPassword = "Wrong",
            NewPassword = "New"
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.IsAny<Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Current password is incorrect"));

        await   Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.ChangePasswordCurrentUserAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "USER_044 - Controller - POST /api/v1/User/delete-account gọi đúng Command")]
    public async Task DeleteAccount_CallsCorrectCommand_ReturnsOk()
    {
        var expectedResponse = new DeleteAccountByUserReponse { Message = "Account deleted successfully" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCurrentUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.DeleteCurrentUserAccountAsync(CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<DeleteCurrentUserAccountCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "USER_045 - Controller - POST /api/v1/User/delete-account xử lý ForbiddenException")]
    public async Task DeleteAccount_ForbiddenException_ReturnsForbiddenObjectResult()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCurrentUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DeleteAccountByUserReponse>.Failure(Error.Forbidden("Cannot delete banned account")));

        var result = await _controller.DeleteCurrentUserAccountAsync(CancellationToken.None).ConfigureAwait(true);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact(DisplayName = "USER_046 - Controller - POST /api/v1/User/{userId}/restore gọi đúng Command")]
    public async Task RestoreAccount_CallsCorrectCommand_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var expectedResponse = new RestoreUserResponse { Message = "Account restored successfully" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.RestoreUserAccountAsync(userId, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(
            m => m.Send(It.Is<RestoreUserAccountCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "USER_047 - Controller - POST /api/v1/User/{userId}/restore với userId không hợp lệ")]
    public async Task RestoreAccount_InvalidGuid_ModelBindingFail()
    {
        var userId = Guid.Empty;

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RestoreUserResponse>.Failure(Error.BadRequest("Invalid user ID")));

        var result = await _controller.RestoreUserAccountAsync(userId, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "USER_048 - Controller - POST /api/v1/User/{userId}/restore xử lý NotFoundException")]
    public async Task RestoreAccount_NotFoundException_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RestoreUserResponse>.Failure(Error.NotFound("User not found")));

        var result = await _controller.RestoreUserAccountAsync(userId, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "USER_049 - Controller - POST /api/v1/User/{userId}/restore xử lý BadRequestException")]
    public async Task RestoreAccount_BadRequestException_ThrowsBadRequestException()
    {
        var userId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RestoreUserResponse>.Failure(Error.BadRequest("Account is not deleted")));

        var result = await _controller.RestoreUserAccountAsync(userId, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "USER_050 - Controller - Kiểm tra Authorization Attribute trên các endpoints")]
    public void Controller_HasAuthorizeAttribute_AllEndpointsProtected()
    {
        var controllerType = typeof(UserController);

        var controllerAttributes = controllerType.GetCustomAttributes(
            typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute),
            true);

        var methods = controllerType.GetMethods(
            System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.DeclaredOnly);

        var hasControllerLevelAuthorize = controllerAttributes.Length > 0;

        if(!hasControllerLevelAuthorize)
        {
            foreach(var method in methods)
            {
                var allowAnonymous = method.GetCustomAttributes(
                    typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute),
                    true);

                if(allowAnonymous.Length > 0)
                    continue;

                var methodAttributes = method.GetCustomAttributes(
                    typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute),
                    true);

                methodAttributes.Should()
                    .NotBeEmpty($"Method {method.Name} should be secured or explicitly AllowAnonymous");
            }
        } else
        {
            hasControllerLevelAuthorize.Should().BeTrue();
        }
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
