using Application.ApiContracts.User.Requests;
using Application.ApiContracts.UserManager.Requests;
using Application.Common.Exceptions;
using Application.Features.UserManager.Commands.AssignRoles;
using Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;
using Application.Features.UserManager.Commands.ChangePassword;
using Application.Features.UserManager.Commands.ChangeUserStatus;
using Application.Features.UserManager.Commands.UpdateUser;
using Application.Features.UserManager.Queries.GetUserById;
using Application.Features.UserManager.Queries.GetUsersList;
using Application.Features.UserManager.Queries.GetUsersListForOutput;
using Application.Interfaces.Repositories.UserManager;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Sieve.Models;
using Xunit;

namespace UnitTests;

public class UserManager
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<IUserManagerReadRepository> _userManagerRepositoryMock;
    private readonly Mock<IProtectedEntityManagerService> _protectedEntityManagerServiceMock;

    public UserManager()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        _userManagerRepositoryMock = new Mock<IUserManagerReadRepository>();
        _protectedEntityManagerServiceMock = new Mock<IProtectedEntityManagerService>();
    }

    [Fact(DisplayName = "UMGR_037 - Validate FullName không được rỗng")]
    public void ValidateFullName_EmptyString_ValidationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FullName = ""
        };
        var validator = new UpdateUserCommandValidator();

        // Act
        var result = validator.Validate(new UpdateUserCommand(userId, request));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("FullName"));
    }

    [Fact(DisplayName = "UMGR_038 - Validate FullName có độ dài tối đa hợp lệ")]
    public void ValidateFullName_ExceedsMaxLength_ValidationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FullName = new string('A', 256)
        };
        var validator = new UpdateUserCommandValidator();

        // Act
        var result = validator.Validate(new UpdateUserCommand(userId, request));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("FullName"));
    }

    [Fact(DisplayName = "UMGR_039 - Validate Email format với các trường hợp không hợp lệ")]
    public void ValidateEmail_InvalidFormats_ValidationFails()
    {
        // Test case 1: plaintext
        UpdateUserCommandValidator.IsValidEmail("plaintext").Should().BeFalse();

        // Test case 2: @example.com
        UpdateUserCommandValidator.IsValidEmail("@example.com").Should().BeFalse();

        // Test case 3: user@
        UpdateUserCommandValidator.IsValidEmail("user@").Should().BeFalse();

        // Test case 4: user @example.com (with space)
        UpdateUserCommandValidator.IsValidEmail("user @example.com").Should().BeFalse();
    }

    [Fact(DisplayName = "UMGR_040 - Validate Email format với các trường hợp hợp lệ")]
    public void ValidateEmail_ValidFormats_ValidationPasses()
    {
        // Test case 1: user@example.com
        UpdateUserCommandValidator.IsValidEmail("user@example.com").Should().BeTrue();

        // Test case 2: user+tag@example.co.uk
        UpdateUserCommandValidator.IsValidEmail("user+tag@example.co.uk").Should().BeTrue();

        // Test case 3: user.name@example.com
        UpdateUserCommandValidator.IsValidEmail("user.name@example.com").Should().BeTrue();

        // Test case 4: user123@test-domain.com
        UpdateUserCommandValidator.IsValidEmail("user123@test-domain.com").Should().BeTrue();
    }

    [Fact(DisplayName = "UMGR_041 - Validate PhoneNumber format Việt Nam")]
    public void ValidatePhoneNumber_VariousFormats_ReturnsCorrectValidation()
    {
        // Test case 1: 0912345678 (valid)
        UpdateUserCommandValidator.IsValidPhoneNumber("0912345678").Should().BeTrue();

        // Test case 2: 84912345678 (valid)
        UpdateUserCommandValidator.IsValidPhoneNumber("84912345678").Should().BeTrue();

        // Test case 3: +84912345678 (valid)
        UpdateUserCommandValidator.IsValidPhoneNumber("+84912345678").Should().BeTrue();

        // Test case 4: 091234 (invalid - too short)
        UpdateUserCommandValidator.IsValidPhoneNumber("091234").Should().BeFalse();

        // Test case 5: abcd123456 (invalid - contains letters)
        UpdateUserCommandValidator.IsValidPhoneNumber("abcd123456").Should().BeFalse();
    }

    [Fact(DisplayName = "UMGR_042 - Validate Gender chỉ chấp nhận các giá trị hợp lệ")]
    public void ValidateGender_VariousValues_ReturnsCorrectValidation()
    {
        // Test case 1: Male (valid)
        UpdateUserCommandValidator.IsValidGender(GenderStatus.Male).Should().BeTrue();

        // Test case 2: Female (valid)
        UpdateUserCommandValidator.IsValidGender(GenderStatus.Female).Should().BeTrue();

        // Test case 3: Other (valid)
        UpdateUserCommandValidator.IsValidGender(GenderStatus.Other).Should().BeTrue();

        // Test case 4: InvalidGender (invalid)
        UpdateUserCommandValidator.IsValidGender("InvalidGender").Should().BeFalse();

        // Test case 5: empty string (invalid)
        UpdateUserCommandValidator.IsValidGender("").Should().BeFalse();
    }

    [Fact(DisplayName = "UMGR_043 - Validate Password strength requirements")]
    public void ValidatePassword_VariousStrengths_ReturnsCorrectValidation()
    {
        // Test case 1: Pass@123 (valid - uppercase, lowercase, number, special char, >= 8 chars)
        ChangePasswordCommandValidator.IsStrongPassword("Pass@123").Should().BeTrue();

        // Test case 2: password (invalid - missing uppercase, number, special char)
        ChangePasswordCommandValidator.IsStrongPassword("password").Should().BeFalse();

        // Test case 3: Pass123 (invalid - missing special char)
        ChangePasswordCommandValidator.IsStrongPassword("Pass123").Should().BeFalse();

        // Test case 4: P@1 (invalid - too short)
        ChangePasswordCommandValidator.IsStrongPassword("P@1").Should().BeFalse();
    }

    [Fact(DisplayName = "UMGR_044 - Validate Status chỉ chấp nhận các giá trị trong UserStatus constant")]
    public void ValidateStatus_VariousValues_ReturnsCorrectValidation()
    {
        // Test case 1: Active (valid)
        ChangeUserStatusCommandValidator.IsValidStatus(UserStatus.Active).Should().BeTrue();

        // Test case 2: Banned (valid)
        ChangeUserStatusCommandValidator.IsValidStatus(UserStatus.Banned).Should().BeTrue();

        // Test case 3: InvalidStatus (invalid)
        ChangeUserStatusCommandValidator.IsValidStatus("InvalidStatus").Should().BeFalse();
    }
}
