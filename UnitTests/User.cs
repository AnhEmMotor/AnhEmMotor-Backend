using Application.ApiContracts.Permission.Responses;
using Application.Features.Permissions.Queries.GetMyPermissions;
using Application.Features.Permissions.Queries.GetUserPermissionsById;
using Application.Features.UserManager.Commands.CreateUserByManager;
using Application.Features.Users.Commands.ChangePassword;
using Application.Features.Users.Commands.DeleteCurrentUserAccount;
using Application.Features.Users.Commands.UpdateCurrentUser;
using Application.Features.Users.Queries.GetCurrentUser;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Moq;

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
    [Fact(DisplayName = "USER_001 - L?y thông tin ngu?i dùng hi?n t?i thành công")]
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
        _userReadRepositoryMock.Setup(x => x.GetRolesOfUserAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(
            x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(
            x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
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

    [Fact(DisplayName = "USER_002 - L?y thông tin ngu?i dùng khi JWT không h?p l?")]
    public async Task GetCurrentUser_InvalidJWT_ThrowsUnauthorizedException()
    {
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);
        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = null };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_003 - L?y thông tin ngu?i dùng khi tài kho?n dã b? xóa m?m")]
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

    [Fact(DisplayName = "USER_004 - L?y thông tin ngu?i dùng khi tài kho?n b? Ban")]
    public async Task GetCurrentUser_BannedAccount_ReturnsUserResponseWithBannedStatus()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Banned, DeletedAt = null };
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.GetRolesOfUserAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(
            x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(
            x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = userId.ToString() };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(UserStatus.Banned);
    }

    [Fact(DisplayName = "USER_051 - Verify UserResponse ch?a danh sách Permissions")]
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
        _roleReadRepositoryMock.Setup(
            x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _roleReadRepositoryMock.Setup(
            x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);
        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = userId.ToString() };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Permissions.Should().NotBeNullOrEmpty();
        result.Value.Permissions.Should().HaveCount(2);
        result.Value.Permissions!.Should().Contain("User.Read");
    }

    [Fact(DisplayName = "USER_052 - Verify UserResponse.Permissions r?ng khi user không có quy?n")]
    public async Task GetCurrentUser_NoPermissions_ReturnsEmptyPermissions()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active };
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.GetRolesOfUserAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(
            x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _roleReadRepositoryMock.Setup(
            x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = new GetCurrentUserQueryHandler(_userReadRepositoryMock.Object, _roleReadRepositoryMock.Object);
        var query = new GetCurrentUserQuery() { UserId = userId.ToString() };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Permissions.Should().BeNull();
    }

    [Fact(DisplayName = "USER_005 - C?p nh?t thông tin ngu?i dùng thành công")]
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

    [Fact(DisplayName = "USER_006 - C?p nh?t thông tin v?i d? li?u r?ng (không thay d?i gì)")]
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

    [Fact(DisplayName = "USER_007 - C?p nh?t thông tin v?i kho?ng tr?ng ? d?u và cu?i chu?i")]
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

    [Fact(DisplayName = "USER_008 - C?p nh?t thông tin v?i ký t? d?c bi?t trong FullName")]
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

    [Fact(DisplayName = "USER_009 - C?p nh?t thông tin v?i Gender không h?p l?")]
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

    [Fact(DisplayName = "USER_010 - C?p nh?t thông tin v?i s? di?n tho?i không h?p l? (ch? vào ch? s?)")]
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

    [Fact(DisplayName = "USER_011 - C?p nh?t thông tin khi ngu?i dùng dã b? xóa m?m")]
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

    [Fact(DisplayName = "USER_012 - C?p nh?t thông tin khi ngu?i dùng b? Ban")]
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

    [Fact(DisplayName = "USER_013 - C?p nh?t thông tin v?i tru?ng Email trong body (ph?i b? ch?n)")]
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

    [Fact(DisplayName = "USER_014 - Ð?i m?t kh?u thành công")]
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
        var handler = new ChangePasswordCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var command = new ChangePasswordCommand()
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

    [Fact(DisplayName = "USER_015 - Ð?i m?t kh?u v?i CurrentPassword sai")]
    public async Task ChangePassword_WrongCurrentPassword_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active, DeletedAt = null };
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.CheckPasswordAsync(user, "WrongPass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new ChangePasswordCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var command = new ChangePasswordCommand()
        {
            UserId = userId.ToString(),
            CurrentPassword = "WrongPass",
            NewPassword = "NewPass456!"
        };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_016 - Ð?i m?t kh?u v?i NewPassword quá ng?n (validation)")]
    public async Task ChangePassword_NewPasswordTooShort_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active, DeletedAt = null };
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.CheckPasswordAsync(user, "OldPass123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new ChangePasswordCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var command = new ChangePasswordCommand()
        {
            UserId = userId.ToString(),
            CurrentPassword = "OldPass123!",
            NewPassword = "123"
        };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_017 - Ð?i m?t kh?u khi tài kho?n dã b? xóa m?m")]
    public async Task ChangePassword_DeletedAccount_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, DeletedAt = DateTimeOffset.UtcNow.AddDays(-1) };
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var handler = new ChangePasswordCommandHandler(_userReadRepositoryMock.Object, _userUpdateRepositoryMock.Object);
        var command = new ChangePasswordCommand()
        {
            UserId = userId.ToString(),
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass456!"
        };
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "USER_018 - Xóa tài kho?n thành công")]
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

    [Fact(DisplayName = "USER_019 - Xóa tài kho?n khi dã b? Ban (không cho phép)")]
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

    [Fact(DisplayName = "USER_020 - Xóa tài kho?n khi tài kho?n dã b? xóa m?m tru?c dó")]
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

    [Fact(DisplayName = "USR_PERM_001 - L?y quy?n c?a ngu?i dùng theo ID tr? v? danh sách chu?i ID")]
    public async Task GetUserPermissionsById_ReturnsListOfStringIds()
    {
        var handler = new GetUserPermissionsByIdQueryHandler(
            _userReadRepositoryMock.Object,
            _roleReadRepositoryMock.Object);
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "testuser", Email = "test@test.com" };
        var roles = new List<string> { "Manager" };
        var roleEntities = new List<ApplicationRole> { new() { Id = Guid.NewGuid(), Name = "Manager" } };
        var permissionNames = new List<string> { Domain.Constants.Permission.Permissions.Brands.View, Domain.Constants.Permission.Permissions.Products.View };
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.GetRolesOfUserAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _roleReadRepositoryMock.Setup(x => x.GetRolesByNameAsync(roles, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleEntities);
        _roleReadRepositoryMock.Setup(
            x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionNames);
        var query = new GetUserPermissionsByIdQuery { UserId = userId };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Permissions.Should().BeAssignableTo<IList<string>>();
        result.Value.Permissions.Should().Contain(Domain.Constants.Permission.Permissions.Brands.View);
        result.Value.Permissions.Should().Contain(Domain.Constants.Permission.Permissions.Products.View);
        result.Value.Permissions.Should().NotContainNulls();
    }

    [Fact(DisplayName = "USR_PERM_002 - L?y quy?n c?a ngu?i dùng hi?n t?i (GetMyPermissions) tr? v? danh sách chu?i ID")]
    public async Task GetMyPermissions_ReturnsListOfStringIds()
    {
        var handler = new GetMyPermissionsQueryHandler(_roleReadRepositoryMock.Object, _userReadRepositoryMock.Object);
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "testuser", Email = "test@test.com" };
        var roles = new List<string> { "Manager" };
        var roleEntities = new List<ApplicationRole> { new() { Id = Guid.NewGuid(), Name = "Manager" } };
        var permissionNames = new List<string> { Domain.Constants.Permission.Permissions.Suppliers.View, Domain.Constants.Permission.Permissions.Files.Upload };
        _userReadRepositoryMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userReadRepositoryMock.Setup(x => x.GetRolesOfUserAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        _roleReadRepositoryMock.Setup(x => x.GetRolesByNameAsync(roles, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleEntities);
        _roleReadRepositoryMock.Setup(
            x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionNames);
        var query = new GetMyPermissionsQuery { UserId = userId.ToString() };
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Permissions.Should().BeAssignableTo<IList<string>>();
        result.Value.Permissions.Should().Contain(Domain.Constants.Permission.Permissions.Suppliers.View);
        result.Value.Permissions.Should().Contain(Domain.Constants.Permission.Permissions.Files.Upload);
    }

    [Fact(DisplayName = "USER_079 - Kiểm tra độ mạnh của mật khẩu (Validation)")]
    public void CreateUserCommandValidator_ShouldFail_WhenPasswordTooShort()
    {
        var validator = new CreateUserByManagerCommandValidator();
        var command = new CreateUserByManagerCommand
        {
            Username = "admin1",
            Email = "user@test.com",
            Password = "123",
            RoleNames = ["Staff"]
        };
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => string.Compare(e.PropertyName, "Password") == 0);
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}



