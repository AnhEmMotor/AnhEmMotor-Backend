using Application.Features.UserManager.Commands.ChangePassword;
using Application.Features.UserManager.Commands.UpdateUser;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class UserManager
{
    private readonly Mock<IUserReadRepository> _userReadRepositoryMock;
    private readonly Mock<IUserUpdateRepository> _userUpdateRepositoryMock;
    private readonly Mock<IUserCreateRepository> _userCreateRepositoryMock;
    private readonly Mock<IUserDeleteRepository> _userDeleteRepositoryMock;

    private readonly Mock<IProtectedEntityManagerService> _protectedEntityManagerServiceMock;

    public UserManager()
    {
        _userReadRepositoryMock = new Mock<IUserReadRepository>();
        _userUpdateRepositoryMock = new Mock<IUserUpdateRepository>();
        _userCreateRepositoryMock = new Mock<IUserCreateRepository>();
        _userDeleteRepositoryMock = new Mock<IUserDeleteRepository>();

        _protectedEntityManagerServiceMock = new Mock<IProtectedEntityManagerService>();
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "UMGR_037 - Validate FullName không được rỗng")]
    public void ValidateFullName_EmptyString_ValidationFails()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand() { UserId = userId, FullName = string.Empty };
        var validator = new UpdateUserCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("FullName"));
    }

    [Fact(DisplayName = "UMGR_038 - Validate FullName có độ dài tối đa hợp lệ")]
    public void ValidateFullName_ExceedsMaxLength_ValidationFails()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand() { UserId = userId, FullName = new string('A', 256) };
        var validator = new UpdateUserCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("FullName"));
    }

    [Fact(DisplayName = "UMGR_039 - Validate Email format với các trường hợp không hợp lệ")]
    public void ValidateEmail_InvalidFormats_ValidationFails()
    {
        UpdateUserCommandValidator.IsValidEmail("plaintext").Should().BeFalse();

        UpdateUserCommandValidator.IsValidEmail("@example.com").Should().BeFalse();

        UpdateUserCommandValidator.IsValidEmail("user@").Should().BeFalse();

        UpdateUserCommandValidator.IsValidEmail("user @example.com").Should().BeFalse();
    }

    [Fact(DisplayName = "UMGR_040 - Validate Email format với các trường hợp hợp lệ")]
    public void ValidateEmail_ValidFormats_ValidationPasses()
    {
        UpdateUserCommandValidator.IsValidEmail("user@example.com").Should().BeTrue();

        UpdateUserCommandValidator.IsValidEmail("user+tag@example.co.uk").Should().BeTrue();

        UpdateUserCommandValidator.IsValidEmail("user.name@example.com").Should().BeTrue();

        UpdateUserCommandValidator.IsValidEmail("user123@test-domain.com").Should().BeTrue();
    }

    [Fact(DisplayName = "UMGR_041 - Validate PhoneNumber format Việt Nam")]
    public void ValidatePhoneNumber_VariousFormats_ReturnsCorrectValidation()
    {
        UpdateUserCommandValidator.IsValidPhoneNumber("0912345678").Should().BeTrue();

        UpdateUserCommandValidator.IsValidPhoneNumber("84912345678").Should().BeTrue();

        UpdateUserCommandValidator.IsValidPhoneNumber("+84912345678").Should().BeTrue();

        UpdateUserCommandValidator.IsValidPhoneNumber("091234").Should().BeFalse();

        UpdateUserCommandValidator.IsValidPhoneNumber("abcd123456").Should().BeFalse();
    }

    [Fact(DisplayName = "UMGR_042 - Validate Gender chỉ chấp nhận các giá trị hợp lệ")]
    public void ValidateGender_VariousValues_ReturnsCorrectValidation()
    {
        UpdateUserCommandValidator.IsValidGender(GenderStatus.Male).Should().BeTrue();

        UpdateUserCommandValidator.IsValidGender(GenderStatus.Female).Should().BeTrue();

        UpdateUserCommandValidator.IsValidGender(GenderStatus.Other).Should().BeTrue();

        UpdateUserCommandValidator.IsValidGender("InvalidGender").Should().BeFalse();

        UpdateUserCommandValidator.IsValidGender(string.Empty).Should().BeFalse();
    }

    [Fact(DisplayName = "UMGR_043 - Validate Password strength requirements")]
    public void ValidatePassword_VariousStrengths_ReturnsCorrectValidation()
    {
        ChangePasswordCommandValidator.IsStrongPassword("Pass@123").Should().BeTrue();

        ChangePasswordCommandValidator.IsStrongPassword("password").Should().BeFalse();

        ChangePasswordCommandValidator.IsStrongPassword("Pass123").Should().BeFalse();

        ChangePasswordCommandValidator.IsStrongPassword("P@1").Should().BeFalse();
    }
#pragma warning restore CRR0035
}
