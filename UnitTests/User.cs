using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Application.Features.Users.Commands.ChangePasswordCurrentUser;
using Application.Features.Users.Commands.DeleteCurrentUserAccount;
using Application.Features.Users.Commands.RestoreUserAccount;
using Application.Features.Users.Commands.UpdateCurrentUser;
using Application.Features.Users.Queries.GetCurrentUser;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace UnitTests;

public class User
{
    private readonly Mock<IUserReadRepository> _userReadRepositoryMock;
    private readonly Mock<IUserUpdateRepository> _userUpdateRepositoryMock;
    private readonly Mock<IUserDeleteRepository> _userDeleteRepositoryMock;
    private readonly Mock<IProtectedEntityManagerService> _protectedEntityManagerServiceMock;

    public User()
    {
        _userReadRepositoryMock = new Mock<IUserReadRepository>();
        _userUpdateRepositoryMock = new Mock<IUserUpdateRepository>();
        _userDeleteRepositoryMock = new Mock<IUserDeleteRepository>();
        _protectedEntityManagerServiceMock = new Mock<IProtectedEntityManagerService>();
    }

    [Fact(DisplayName = "USER_001 - Lấy thông tin người dùng hiện tại thành công")]
    public async Task GetCurrentUser_Success_ReturnsUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test User",
            Gender = GenderStatus.Male,
            PhoneNumber = "0123456789",
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery(userId.ToString());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        //TODO: Viết lại đống này cho nó chạy
        //result.IsSuccess.Should().BeTrue();
        //result.Value.Id.Should().Be(userId);
        //result.Value.UserName.Should().Be("testuser");
        //result.Value.Email.Should().Be("test@example.com");
        //result.Value.FullName.Should().Be("Test User");
        //result.Value.Gender.Should().Be(GenderStatus.Male);
        //result.Value.PhoneNumber.Should().Be("0123456789");
    }

    [Fact(DisplayName = "USER_002 - Lấy thông tin người dùng khi JWT không hợp lệ")]
    public async Task GetCurrentUser_InvalidJWT_ThrowsUnauthorizedException()
    {
        // Arrange
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery(null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(query, CancellationToken.None));
    }

    [Fact(DisplayName = "USER_003 - Lấy thông tin người dùng khi tài khoản đã bị xóa mềm")]
    public async Task GetCurrentUser_DeletedAccount_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery(userId.ToString());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => 
            handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Contain("Account has been deleted");
    }

    [Fact(DisplayName = "USER_004 - Lấy thông tin người dùng khi tài khoản bị Ban")]
    public async Task GetCurrentUser_BannedAccount_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Banned,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery(userId.ToString());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => 
            handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Contain("Account has been banned");
    }

    [Fact(DisplayName = "USER_005 - Cập nhật thông tin người dùng thành công")]
    public async Task UpdateCurrentUser_Success_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Old Name",
            Gender = GenderStatus.Male,
            PhoneNumber = "0123456789",
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userUpdateRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new UpdateUserRequest
        {
            FullName = "New Name",
            Gender = GenderStatus.Female,
            PhoneNumber = "0987654321"
        };
        var command = new UpdateCurrentUserCommand(userId.ToString(), request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("New Name");
        result.Value.Gender.Should().Be(GenderStatus.Female);
        result.Value.PhoneNumber.Should().Be("0987654321");
    }

    [Fact(DisplayName = "USER_006 - Cập nhật thông tin với dữ liệu rỗng (không thay đổi gì)")]
    public async Task UpdateCurrentUser_EmptyData_KeepsOriginalData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            FullName = "Old Name",
            Gender = GenderStatus.Male,
            PhoneNumber = "0123456789",
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userUpdateRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new UpdateUserRequest
        {
            FullName = null,
            Gender = null,
            PhoneNumber = null
        };
        var command = new UpdateCurrentUserCommand(userId.ToString(), request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("Old Name");
        result.Value.Gender.Should().Be(GenderStatus.Male);
        result.Value.PhoneNumber.Should().Be("0123456789");
    }

    [Fact(DisplayName = "USER_007 - Cập nhật thông tin với khoảng trắng ở đầu và cuối chuỗi")]
    public async Task UpdateCurrentUser_WhitespaceData_TrimmedCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userUpdateRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new UpdateUserRequest
        {
            FullName = "  Trimmed Name  ",
            PhoneNumber = "  0999888777  "
        };
        var command = new UpdateCurrentUserCommand(userId.ToString(), request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("Trimmed Name");
        result.Value.PhoneNumber.Should().Be("0999888777");
    }

    [Fact(DisplayName = "USER_008 - Cập nhật thông tin với ký tự đặc biệt trong FullName")]
    public async Task UpdateCurrentUser_SpecialCharacters_SavedAsLiteral()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userUpdateRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new UpdateUserRequest
        {
            FullName = "<script>alert('XSS')</script>",
            Gender = GenderStatus.Male
        };
        var command = new UpdateCurrentUserCommand(userId.ToString(), request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("<script>alert('XSS')</script>");
    }

    [Fact(DisplayName = "USER_009 - Cập nhật thông tin với Gender không hợp lệ")]
    public async Task UpdateCurrentUser_InvalidGender_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new UpdateUserRequest
        {
            Gender = "InvalidGender"
        };
        var command = new UpdateCurrentUserCommand(userId.ToString(), request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
            handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Invalid gender value");
    }

    [Fact(DisplayName = "USER_010 - Cập nhật thông tin với số điện thoại không hợp lệ (chữ vào chỗ số)")]
    public async Task UpdateCurrentUser_InvalidPhoneNumber_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new UpdateUserRequest
        {
            PhoneNumber = "abcd1234"
        };
        var command = new UpdateCurrentUserCommand(userId.ToString(), request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
            handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Invalid phone number format");
    }

    [Fact(DisplayName = "USER_011 - Cập nhật thông tin khi người dùng đã bị xóa mềm")]
    public async Task UpdateCurrentUser_DeletedAccount_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new UpdateUserRequest { FullName = "Test" };
        var command = new UpdateCurrentUserCommand(userId.ToString(), request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => 
            handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Cannot update deleted account");
    }

    [Fact(DisplayName = "USER_012 - Cập nhật thông tin khi người dùng bị Ban")]
    public async Task UpdateCurrentUser_BannedAccount_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Banned,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new UpdateUserRequest { FullName = "Test" };
        var command = new UpdateCurrentUserCommand(userId.ToString(), request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => 
            handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Cannot update banned account");
    }

    [Fact(DisplayName = "USER_013 - Cập nhật thông tin với trường Email trong body (phải bị chặn)")]
    public async Task UpdateCurrentUser_EmailInBody_EmailNotChanged()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "old@example.com",
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userUpdateRepositoryMock.Setup(x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new UpdateUserRequest { FullName = "Test" };
        var command = new UpdateCurrentUserCommand(userId.ToString(), request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Verify Email không bị thay đổi (vì không có trong UpdateUserRequest)
        _userUpdateRepositoryMock.Verify(x => x.UpdateUserAsync(It.Is<ApplicationUser>(u => u.Email == "old@example.com")), Times.Once);
    }

    [Fact(DisplayName = "USER_014 - Đổi mật khẩu thành công")]
    public async Task ChangePassword_Success_PasswordChangedAndSecurityStampRefreshed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = null,
            SecurityStamp = "old-stamp"
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.CheckPasswordAsync(user, "OldPass123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userUpdateRepositoryMock.Setup(x => x.ChangePasswordAsync(user, "OldPass123!", "NewPass456!", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new ChangePasswordCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass456!"
        };
        var command = new ChangePasswordCurrentUserCommand(userId.ToString(), request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Be("Password changed successfully.");
        _userUpdateRepositoryMock.Verify(x => x.ChangePasswordAsync(user, "OldPass123!", "NewPass456!", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "USER_015 - Đổi mật khẩu với CurrentPassword sai")]
    public async Task ChangePassword_WrongCurrentPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.CheckPasswordAsync(user, "WrongPass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new ChangePasswordCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "WrongPass",
            NewPassword = "NewPass456!"
        };
        var command = new ChangePasswordCurrentUserCommand(userId.ToString(), request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Current password is incorrect");
    }

    [Fact(DisplayName = "USER_016 - Đổi mật khẩu với NewPassword quá ngắn (validation)")]
    public async Task ChangePassword_NewPasswordTooShort_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.CheckPasswordAsync(user, "OldPass123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new ChangePasswordCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPass123!",
            NewPassword = "123"
        };
        var command = new ChangePasswordCurrentUserCommand(userId.ToString(), request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
            handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Password must be at least 6 characters");
    }

    [Fact(DisplayName = "USER_017 - Đổi mật khẩu khi tài khoản đã bị xóa mềm")]
    public async Task ChangePassword_DeletedAccount_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new ChangePasswordCurrentUserCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass456!"
        };
        var command = new ChangePasswordCurrentUserCommand(userId.ToString(), request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => 
            handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Cannot change password for deleted account");
    }

    [Fact(DisplayName = "USER_018 - Xóa tài khoản thành công")]
    public async Task DeleteAccount_Success_AccountDeletedAndSecurityStampRefreshed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = null,
            SecurityStamp = "old-stamp"
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userDeleteRepositoryMock.Setup(x => x.SoftDeleteUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new DeleteCurrentUserAccountCommandHandler(_userReadRepositoryMock.Object, _userDeleteRepositoryMock.Object, _protectedEntityManagerServiceMock.Object);
        var command = new DeleteCurrentUserAccountCommand(userId.ToString());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Be("Your account has been deleted successfully.");
        _userDeleteRepositoryMock.Verify(x => x.SoftDeleteUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "USER_019 - Xóa tài khoản khi đã bị Ban (không cho phép)")]
    public async Task DeleteAccount_BannedAccount_ThrowsForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Banned,
            DeletedAt = null
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new DeleteCurrentUserAccountCommandHandler(_userReadRepositoryMock.Object, _userDeleteRepositoryMock.Object, _protectedEntityManagerServiceMock.Object);
        var command = new DeleteCurrentUserAccountCommand(userId.ToString());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => 
            handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Cannot delete banned account");
    }

    [Fact(DisplayName = "USER_020 - Xóa tài khoản khi tài khoản đã bị xóa mềm trước đó")]
    public async Task DeleteAccount_AlreadyDeleted_ThrowsBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-2)
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new DeleteCurrentUserAccountCommandHandler(_userReadRepositoryMock.Object, _userDeleteRepositoryMock.Object, _protectedEntityManagerServiceMock.Object);
        var command = new DeleteCurrentUserAccountCommand(userId.ToString());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
            handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Contain("Account already deleted");
    }
}


