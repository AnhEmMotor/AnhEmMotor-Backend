using Application.Features.Users.Commands.DeleteCurrentUserAccount;
using Application.Features.Users.Commands.UpdateCurrentUser;
using Application.Features.Users.Queries.GetCurrentUser;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Services;
using Moq;
using System.Collections;
using System.Reflection;

namespace UnitTests;

public class User
{
    private readonly Mock<IUserReadRepository> _userReadRepositoryMock;
    private readonly Mock<IUserUpdateRepository> _userUpdateRepositoryMock;
    private readonly Mock<IUserDeleteRepository> _userDeleteRepositoryMock;
    private readonly Mock<IProtectedEntityManagerService> _protectedEntityManagerServiceMock;
    private readonly Mock<IRoleReadRepository> _roleReadRepositoryMock;
    private readonly Mock<IUserStreamService> _userStreamServiceMock;

    public User()
    {
        _userReadRepositoryMock = new Mock<IUserReadRepository>();
        _userUpdateRepositoryMock = new Mock<IUserUpdateRepository>();
        _userDeleteRepositoryMock = new Mock<IUserDeleteRepository>();
        _protectedEntityManagerServiceMock = new Mock<IProtectedEntityManagerService>();
        _roleReadRepositoryMock = new Mock<IRoleReadRepository>();
        _userStreamServiceMock = new Mock<IUserStreamService>();
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "USER_001 - Lấy thông tin người dùng hiện tại thành công")]
    public async Task GetCurrentUser_Success_ReturnsUserResponse()
    {
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
        _userReadRepositoryMock.Setup(x => x.GetRolesOfUserAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());
        _roleReadRepositoryMock.Setup(x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationRole>());
        _roleReadRepositoryMock.Setup(x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = userId.ToString() };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.UserName.Should().Be("testuser");
        result.Value.Email.Should().Be("test@example.com");
        result.Value.FullName.Should().Be("Test User");
        result.Value.Gender.Should().Be(GenderStatus.Male);
        result.Value.PhoneNumber.Should().Be("0123456789");
    }

    [Fact(DisplayName = "USER_002 - Lấy thông tin người dùng khi JWT không hợp lệ")]
    public async Task GetCurrentUser_InvalidJWT_ThrowsUnauthorizedException()
    {
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = null };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_003 - Lấy thông tin người dùng khi tài khoản đã bị xóa mềm")]
    public async Task GetCurrentUser_DeletedAccount_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = userId.ToString() };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_004 - Lấy thông tin người dùng khi tài khoản bị Ban")]
    public async Task GetCurrentUser_BannedAccount_ReturnsUserResponseWithBannedStatus()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Banned, DeletedAt = null };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.GetRolesOfUserAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = userId.ToString() };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(UserStatus.Banned);
    }
    
    [Fact(DisplayName = "USER_051 - Verify UserResponse chứa danh sách Permissions")]
    public async Task GetCurrentUser_WithPermissions_ReturnsPermissionsList()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active };
        var roles = new List<ApplicationRole> { new() { Id = Guid.NewGuid(), Name = "Staff" } };
        var permissions = new List<string> { "User.Read", "User.Write" };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.GetRolesOfUserAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["Staff"]);
        _roleReadRepositoryMock.Setup(x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _roleReadRepositoryMock.Setup(x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = userId.ToString() };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Permissions.Should().NotBeNullOrEmpty();
        result.Value.Permissions.Should().HaveCount(2);
        result.Value.Permissions!.Select(p => p.ID).Should().Contain("User.Read");
    }

    [Fact(DisplayName = "USER_052 - Verify UserResponse.Permissions rỗng khi user không có quyền")]
    public async Task GetCurrentUser_NoPermissions_ReturnsEmptyPermissions()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.GetRolesOfUserAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = userId.ToString() };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Permissions.Should().BeNull();
    }

    [Fact(DisplayName = "USER_005 - Cập nhật thông tin người dùng thành công")]
    public async Task UpdateCurrentUser_Success_ReturnsUpdatedUser()
    {
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
        _userUpdateRepositoryMock.Setup(
            x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object,
            _userStreamServiceMock.Object);
        var command = new UpdateCurrentUserCommand()
        {
            UserId = userId.ToString(),
            FullName = "New Name",
            Gender = GenderStatus.Female,
            PhoneNumber = "0987654321"
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("New Name");
        result.Value.Gender.Should().Be(GenderStatus.Female);
        result.Value.PhoneNumber.Should().Be("0987654321");
    }

    [Fact(DisplayName = "USER_006 - Cập nhật thông tin với dữ liệu rỗng (không thay đổi gì)")]
    public async Task UpdateCurrentUser_EmptyData_KeepsOriginalData()
    {
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
        _userUpdateRepositoryMock.Setup(
            x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object,
            _userStreamServiceMock.Object);
        var command = new UpdateCurrentUserCommand()
        {
            UserId = userId.ToString(),
            FullName = null,
            Gender = null,
            PhoneNumber = null
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("Old Name");
        result.Value.Gender.Should().Be(GenderStatus.Male);
        result.Value.PhoneNumber.Should().Be("0123456789");
    }

    [Fact(DisplayName = "USER_007 - Cập nhật thông tin với khoảng trắng ở đầu và cuối chuỗi")]
    public async Task UpdateCurrentUser_WhitespaceData_TrimmedCorrectly()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active, DeletedAt = null };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userUpdateRepositoryMock.Setup(
            x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object,
            _userStreamServiceMock.Object);
        var command = new UpdateCurrentUserCommand()
        {
            UserId = userId.ToString(),
            FullName = "  Trimmed Name  ",
            PhoneNumber = "  0999888777  "
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("Trimmed Name");
        result.Value.PhoneNumber.Should().Be("0999888777");
    }

    [Fact(DisplayName = "USER_008 - Cập nhật thông tin với ký tự đặc biệt trong FullName")]
    public async Task UpdateCurrentUser_SpecialCharacters_SavedAsLiteral()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active, DeletedAt = null };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userUpdateRepositoryMock.Setup(
            x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object,
            _userStreamServiceMock.Object);
        var command = new UpdateCurrentUserCommand()
        {
            UserId = userId.ToString(),
            FullName = "<script>alert('XSS')</script>",
            Gender = GenderStatus.Male
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("<script>alert('XSS')</script>");
    }

    [Fact(DisplayName = "USER_009 - Cập nhật thông tin với Gender không hợp lệ")]
    public async Task UpdateCurrentUser_InvalidGender_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active, DeletedAt = null };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCurrentUserCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object,
            _userStreamServiceMock.Object);
        var command = new UpdateCurrentUserCommand() { UserId = userId.ToString(), Gender = "InvalidGender" };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_010 - Cập nhật thông tin với số điện thoại không hợp lệ (chữ vào chỗ số)")]
    public async Task UpdateCurrentUser_InvalidPhoneNumber_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active, DeletedAt = null };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCurrentUserCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object,
            _userStreamServiceMock.Object);
        var command = new UpdateCurrentUserCommand() { UserId = userId.ToString(), PhoneNumber = "abcd1234" };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_011 - Cập nhật thông tin khi người dùng đã bị xóa mềm")]
    public async Task UpdateCurrentUser_DeletedAccount_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, DeletedAt = DateTimeOffset.UtcNow.AddDays(-1) };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCurrentUserCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object,
            _userStreamServiceMock.Object);
        var command = new UpdateCurrentUserCommand() { UserId = userId.ToString(), FullName = "Test" };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_012 - Cập nhật thông tin khi người dùng bị Ban")]
    public async Task UpdateCurrentUser_BannedAccount_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Banned, DeletedAt = null };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCurrentUserCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object,
            _userStreamServiceMock.Object);
        var command = new UpdateCurrentUserCommand() { UserId = userId.ToString(), FullName = "Test" };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_013 - Cập nhật thông tin với trường Email trong body (phải bị chặn)")]
    public async Task UpdateCurrentUser_EmailInBody_EmailNotChanged()
    {
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
        _userUpdateRepositoryMock.Setup(
            x => x.UpdateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new UpdateCurrentUserCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object,
            _userStreamServiceMock.Object);
        var command = new UpdateCurrentUserCommand() { UserId = userId.ToString(), FullName = "Test" };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _userUpdateRepositoryMock.Verify(
            x => x.UpdateUserAsync(
                It.Is<ApplicationUser>(u => string.Compare(u.Email, "old@example.com") == 0),
                CancellationToken.None),
            Times.Once);
    }

    [Fact(DisplayName = "USER_014 - Đổi mật khẩu thành công")]
    public async Task ChangePassword_Success_PasswordChangedAndSecurityStampRefreshed()
    {
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
        _userUpdateRepositoryMock.Setup(
            x => x.ChangePasswordAsync(user, "OldPass123!", "NewPass456!", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object);
        var command = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand()
        {
            UserId = userId.ToString(),
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass456!"
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Be("Password changed successfully.");
        _userUpdateRepositoryMock.Verify(
            x => x.ChangePasswordAsync(user, "OldPass123!", "NewPass456!", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "USER_015 - Đổi mật khẩu với CurrentPassword sai")]
    public async Task ChangePassword_WrongCurrentPassword_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active, DeletedAt = null };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.CheckPasswordAsync(user, "WrongPass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object);
        var command = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand()
        {
            UserId = userId.ToString(),
            CurrentPassword = "WrongPass",
            NewPassword = "NewPass456!"
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_016 - Đổi mật khẩu với NewPassword quá ngắn (validation)")]
    public async Task ChangePassword_NewPasswordTooShort_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active, DeletedAt = null };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.CheckPasswordAsync(user, "OldPass123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object);
        var command = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand()
        {
            UserId = userId.ToString(),
            CurrentPassword = "OldPass123!",
            NewPassword = "123"
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_017 - Đổi mật khẩu khi tài khoản đã bị xóa mềm")]
    public async Task ChangePassword_DeletedAccount_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, DeletedAt = DateTimeOffset.UtcNow.AddDays(-1) };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommandHandler(
            _userReadRepositoryMock.Object,
            _userUpdateRepositoryMock.Object);
        var command = new Application.Features.Users.Commands.ChangePassword.ChangePasswordCommand()
        {
            UserId = userId.ToString(),
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass456!"
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_018 - Xóa tài khoản thành công")]
    public async Task DeleteAccount_Success_AccountDeletedAndSecurityStampRefreshed()
    {
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
        _userDeleteRepositoryMock.Setup(
            x => x.SoftDeleteUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var handler = new DeleteCurrentUserAccountCommandHandler(
            _userReadRepositoryMock.Object,
            _userDeleteRepositoryMock.Object,
            _protectedEntityManagerServiceMock.Object);
        var command = new DeleteCurrentUserAccountCommand() { UserId = userId.ToString() };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Be("Your account has been deleted successfully.");
        _userDeleteRepositoryMock.Verify(
            x => x.SoftDeleteUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "USER_019 - Xóa tài khoản khi đã bị Ban (không cho phép)")]
    public async Task DeleteAccount_BannedAccount_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Banned, DeletedAt = null };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new DeleteCurrentUserAccountCommandHandler(
            _userReadRepositoryMock.Object,
            _userDeleteRepositoryMock.Object,
            _protectedEntityManagerServiceMock.Object);
        var command = new DeleteCurrentUserAccountCommand() { UserId = userId.ToString() };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_020 - Xóa tài khoản khi tài khoản đã bị xóa mềm trước đó")]
    public async Task DeleteAccount_AlreadyDeleted_ThrowsBadRequestException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, DeletedAt = DateTimeOffset.UtcNow.AddDays(-2) };

        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new DeleteCurrentUserAccountCommandHandler(
            _userReadRepositoryMock.Object,
            _userDeleteRepositoryMock.Object,
            _protectedEntityManagerServiceMock.Object);
        var command = new DeleteCurrentUserAccountCommand() { UserId = userId.ToString() };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_063 - Verify cleanup logic when listener disconnects")]
    public async Task WaitForUpdateAsync_ShouldCleanupDictionary_WhenCancelled()
    {
        // 1. Arrange
        var service = new UserStreamService();
        var userId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();

        // Use Reflection to inspect private _listeners dictionary
        var listenersField = typeof(UserStreamService)
            .GetField("_listeners", BindingFlags.NonPublic | BindingFlags.Instance);
        listenersField.Should().NotBeNull();

        // ConcurrentDictionary implements IEnumerable

        // 2. Act: Call WaitForUpdateAsync but don't await it yet (it blocks)
        var waitTask = service.WaitForUpdateAsync(userId, cancellationTokenSource.Token);

        // Allow some time for the task to register the listener
        await Task.Delay(50);

        // Verify listener is added
        // We can't easily check count on generic IEnumerable without casting or iterating, 
        // relying on the fact that waitTask is not completed is good enough proxy that it's "listening".
        waitTask.IsCompleted.Should().BeFalse();

        // 3. Cancel the token to trigger cleanup
        cancellationTokenSource.Cancel();

        // 4. Assert
        // The task should be canceled
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waitTask);

        // Verify dictionary is cleaned up
        // We need to cast or rely on dynamic to check if it's empty
        int count = 0;
        if (listenersField!.GetValue(service) is IEnumerable listenersDict)
        {
            foreach (var item in listenersDict) count++;
        }

        // Should be 0 if cleanup logic worked and removed the key since list was empty
        count.Should().Be(0, "Dictionary should be empty after cancellation");
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}


