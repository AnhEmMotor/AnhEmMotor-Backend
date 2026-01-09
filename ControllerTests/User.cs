using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Application.Features.Users.Commands.ChangePasswordCurrentUser;
using Application.Features.Users.Commands.DeleteCurrentUserAccount;
using Application.Features.Users.Commands.RestoreUserAccount;
using Application.Features.Users.Commands.UpdateCurrentUser;
using Application.Features.Users.Queries.GetCurrentUser;
using Domain.Constants;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;
using Xunit;

namespace ControllerTests;

public class User
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UserController _controller;

    public User()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UserController(_mediatorMock.Object);

        // Setup Controller Context
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "USER_036 - Controller - GET /api/v1/User/me gọi đúng Query")]
    public async Task GetCurrentUser_CallsCorrectQuery_ReturnsOk()
    {
        // Arrange
        var expectedResponse = new UserResponse
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test User",
            Gender = GenderStatus.Male,
            PhoneNumber = "0123456789"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCurrentUser(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "USER_037 - Controller - GET /api/v1/User/me xử lý exception")]
    public async Task GetCurrentUser_HandlesException_ThrowsUnauthorizedException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid JWT"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.GetCurrentUser(CancellationToken.None));
    }

    [Fact(DisplayName = "USER_038 - Controller - PUT /api/v1/User/me gọi đúng Command")]
    public async Task UpdateCurrentUser_CallsCorrectCommand_ReturnsOk()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            FullName = "Test",
            Gender = GenderStatus.Male
        };

        var expectedResponse = new UserDTOForManagerResponse
        {
            Id = Guid.NewGuid(),
            FullName = "Test",
            Gender = GenderStatus.Male
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCurrentUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateCurrentUser(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateCurrentUserCommand>(c => c.Model == request), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "USER_039 - Controller - PUT /api/v1/User/me với body null hoặc rỗng")]
    public async Task UpdateCurrentUser_EmptyBody_CallsMediator()
    {
        // Arrange
        var request = new UpdateUserRequest();

        var expectedResponse = new UserDTOForManagerResponse
        {
            Id = Guid.NewGuid()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCurrentUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateCurrentUser(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateCurrentUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "USER_040 - Controller - PUT /api/v1/User/me xử lý ValidationException")]
    public async Task UpdateCurrentUser_ValidationException_ThrowsValidationException()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            PhoneNumber = "invalid"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCurrentUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid phone number format"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _controller.UpdateCurrentUser(request, CancellationToken.None));
    }

    [Fact(DisplayName = "USER_041 - Controller - POST /api/v1/User/change-password gọi đúng Command")]
    public async Task ChangePassword_CallsCorrectCommand_ReturnsOk()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Old",
            NewPassword = "New"
        };

        var expectedResponse = new ChangePasswordUserByUserResponse
        {
            Message = "Password changed successfully"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCurrentUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ChangePasswordCurrentUser(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(m => m.Send(
            It.Is<ChangePasswordCurrentUserCommand>(c => c.Model == request), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "USER_042 - Controller - POST /api/v1/User/change-password với body thiếu trường")]
    public async Task ChangePassword_MissingField_ThrowsValidationException()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Old",
            NewPassword = "" // Missing
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCurrentUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("NewPassword is required"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _controller.ChangePasswordCurrentUser(request, CancellationToken.None));
    }

    [Fact(DisplayName = "USER_043 - Controller - POST /api/v1/User/change-password xử lý UnauthorizedException")]
    public async Task ChangePassword_UnauthorizedException_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Wrong",
            NewPassword = "New"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCurrentUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Current password is incorrect"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.ChangePasswordCurrentUser(request, CancellationToken.None));
    }

    [Fact(DisplayName = "USER_044 - Controller - POST /api/v1/User/delete-account gọi đúng Command")]
    public async Task DeleteAccount_CallsCorrectCommand_ReturnsOk()
    {
        // Arrange
        var expectedResponse = new DeleteUserByUserReponse
        {
            Message = "Account deleted successfully"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCurrentUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.DeleteCurrentUserAccount(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteCurrentUserAccountCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "USER_045 - Controller - POST /api/v1/User/delete-account xử lý ForbiddenException")]
    public async Task DeleteAccount_ForbiddenException_ThrowsForbiddenException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCurrentUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("Cannot delete banned account"));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _controller.DeleteCurrentUserAccount(CancellationToken.None));
    }

    [Fact(DisplayName = "USER_046 - Controller - POST /api/v1/User/{userId}/restore gọi đúng Command")]
    public async Task RestoreAccount_CallsCorrectCommand_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedResponse = new RestoreUserResponse
        {
            Message = "Account restored successfully"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.RestoreUserAccount(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mediatorMock.Verify(m => m.Send(
            It.Is<RestoreUserAccountCommand>(c => c.UserId == userId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "USER_047 - Controller - POST /api/v1/User/{userId}/restore với userId không hợp lệ")]
    public async Task RestoreAccount_InvalidGuid_ModelBindingFail()
    {
        // Arrange
        // In ASP.NET Core, invalid GUID in route will cause model binding to fail automatically
        // and return 400 Bad Request before hitting the controller action.
        // This test verifies that the controller expects a Guid parameter.
        
        var userId = Guid.Empty; // Empty GUID can be used to simulate invalid

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid user ID"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _controller.RestoreUserAccount(userId, CancellationToken.None));
    }

    [Fact(DisplayName = "USER_048 - Controller - POST /api/v1/User/{userId}/restore xử lý NotFoundException")]
    public async Task RestoreAccount_NotFoundException_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User not found"));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _controller.RestoreUserAccount(userId, CancellationToken.None));
    }

    [Fact(DisplayName = "USER_049 - Controller - POST /api/v1/User/{userId}/restore xử lý BadRequestException")]
    public async Task RestoreAccount_BadRequestException_ThrowsBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreUserAccountCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Account is not deleted"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _controller.RestoreUserAccount(userId, CancellationToken.None));
    }

    [Fact(DisplayName = "USER_050 - Controller - Kiểm tra Authorization Attribute trên các endpoints")]
    public void Controller_HasAuthorizeAttribute_AllEndpointsProtected()
    {
        // Arrange
        var controllerType = typeof(UserController);

        // Act
        var controllerAttributes = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
        
        var methods = controllerType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

        // Assert
        // Either the controller has [Authorize] at class level, or each method has it
        var hasControllerLevelAuthorize = controllerAttributes.Length > 0;

        if (!hasControllerLevelAuthorize)
        {
            // Check each method
            foreach (var method in methods)
            {
                var methodAttributes = method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
                methodAttributes.Should().NotBeEmpty($"Method {method.Name} should have [Authorize] attribute");
            }
        }
        else
        {
            // Controller level authorize is sufficient
            hasControllerLevelAuthorize.Should().BeTrue();
        }
    }
}
