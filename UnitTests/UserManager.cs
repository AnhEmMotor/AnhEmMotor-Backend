using Application.Features.UserManager.Commands.ChangePasswordByManager;
using Application.Features.UserManager.Commands.UpdateUser;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace UnitTests;

public class UserManager
{
    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(DisplayName = "UMGR_037 - Validate FullName rỗng vẫn hợp lệ")]
    public void ValidateFullName_EmptyString_ValidationPasses()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand() { UserId = userId, FullName = string.Empty };
        var validator = new UpdateUserCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
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
        var validator = new UpdateUserCommandValidator();
        var validCommand1 = new UpdateUserCommand { PhoneNumber = "0912345678" };
        validator.TestValidate(validCommand1).ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        var validCommand2 = new UpdateUserCommand { PhoneNumber = "84912345678" };
        validator.TestValidate(validCommand2).ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        var validCommand3 = new UpdateUserCommand { PhoneNumber = "+84912345678" };
        validator.TestValidate(validCommand3).ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        var invalidCommand1 = new UpdateUserCommand { PhoneNumber = "091234" };
        validator.TestValidate(invalidCommand1).ShouldHaveValidationErrorFor(x => x.PhoneNumber);
        var invalidCommand2 = new UpdateUserCommand { PhoneNumber = "abcd123456" };
        validator.TestValidate(invalidCommand2).ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact(DisplayName = "UMGR_042 - Validate Gender chỉ chấp nhận các giá trị hợp lệ hoặc rỗng")]
    public void ValidateGender_VariousValues_ReturnsCorrectValidation()
    {
        UpdateUserCommandValidator.IsValidGender(GenderStatus.Male).Should().BeTrue();
        UpdateUserCommandValidator.IsValidGender(GenderStatus.Female).Should().BeTrue();
        UpdateUserCommandValidator.IsValidGender(GenderStatus.Other).Should().BeTrue();
        UpdateUserCommandValidator.IsValidGender("InvalidGender").Should().BeFalse();
        UpdateUserCommandValidator.IsValidGender(string.Empty).Should().BeFalse();
        var validator = new UpdateUserCommandValidator();
        var command = new UpdateUserCommand { Gender = string.Empty };
        validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    [Fact(DisplayName = "UMGR_043 - Validate Password strength requirements")]
    public void ValidatePassword_VariousStrengths_ReturnsCorrectValidation()
    {
        ChangePasswordByManagerCommandValidator.IsStrongPassword("Pass@123").Should().BeTrue();
        ChangePasswordByManagerCommandValidator.IsStrongPassword("password").Should().BeFalse();
        ChangePasswordByManagerCommandValidator.IsStrongPassword("Pass123").Should().BeFalse();
        ChangePasswordByManagerCommandValidator.IsStrongPassword("P@1").Should().BeFalse();
    }
    [Fact(DisplayName = "USER_074 - Kiểm tra logic Trim dữ liệu đầu vào")]
    public void CreateUserCommand_TrimsUsernameAndEmail()
    {
        // Arrange
        var command = new Application.Features.UserManager.Commands.CreateUserByManager.CreateUserByManagerCommand
        {
            Username = "  admin1  ",
            Email = " user@test.com  "
        };

        // Assert
        command.Username.Should().Be("admin1");
        command.Email.Should().Be("user@test.com");
    }

    [Fact(DisplayName = "USER_075 - Tuyệt đối không Trim mật khẩu")]
    public void CreateUserCommand_DoesNotTrimPassword()
    {
        // Arrange
        var password = " pass 123 ";
        var command = new Application.Features.UserManager.Commands.CreateUserByManager.CreateUserByManagerCommand
        {
            Password = password
        };

        // Assert
        command.Password.Should().Be(password);
    }

    [Fact(DisplayName = "USER_079 - Kiểm tra độ mạnh của mật khẩu (Validation)")]
    public void CreateUserCommandValidator_ShouldFail_WhenPasswordTooShort()
    {
        // Arrange
        var validator = new Application.Features.UserManager.Commands.CreateUserByManager.CreateUserByManagerCommandValidator();
        var command = new Application.Features.UserManager.Commands.CreateUserByManager.CreateUserByManagerCommand
        {
            Username = "admin1",
            Email = "user@test.com",
            Password = "123", // Too short
            RoleNames = ["Staff"]
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
