using Application.Features.Auth.Commands.Register;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentValidation.TestHelper;
using Moq;

namespace UnitTests;

public class Auth
{
#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "AUTH_REG_004 - Validate Format (Unit) - TH1: Email sai định dạng")]
    public async Task AUTH_REG_004_1_Register_InvalidEmail()
    {
        var userReadRepositoryMock = new Mock<IUserReadRepository>();
        var validator = new RegisterCommandValidator(userReadRepositoryMock.Object);

        var command = new RegisterCommand
        {
            Username = "user",
            Email = "invalid-email",
            Password = "Pass",
            FullName = "Full Name",
            PhoneNumber = "0123456789",
            Gender = "Male"
        };

        var result = await validator.TestValidateAsync(command, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact(DisplayName = "AUTH_REG_004 - Validate Format (Unit) - TH2: Password ngắn")]
    public async Task AUTH_REG_004_2_Register_PasswordTooShort()
    {
        var userReadRepositoryMock = new Mock<IUserReadRepository>();
        var validator = new RegisterCommandValidator(userReadRepositoryMock.Object);

        var command = new RegisterCommand
        {
            Username = "user",
            Email = "test@test.com",
            Password = "123",
            FullName = "Full Name",
            PhoneNumber = "0123456789",
            Gender = "Male"
        };

        var result = await validator.TestValidateAsync(command, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact(DisplayName = "AUTH_REG_004 - Validate Format (Unit) - TH3: Username chứa ký tự đặc biệt")]
    public async Task AUTH_REG_004_3_Register_UsernameSpecialChars()
    {
        var userReadRepositoryMock = new Mock<IUserReadRepository>();
        var validator = new RegisterCommandValidator(userReadRepositoryMock.Object);

        var command = new RegisterCommand
        {
            Username = "user@#$",
            Email = "test@test.com",
            Password = "Password123!",
            FullName = "Full Name",
            PhoneNumber = "0123456789",
            Gender = "Male"
        };

        var result = await validator.TestValidateAsync(command, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact(DisplayName = "AUTH_UNI_001 - Xử lý Exception")]
    public async Task AUTH_UNI_001_Exception_Handling()
    {
        var userCreateRepositoryMock = new Mock<IUserCreateRepository>();

        userCreateRepositoryMock.Setup(
            x => x.CreateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Connection Failed"));

        var protectedEntityManagerServiceMock = new Mock<IProtectedEntityManagerService>();
        var handler = new RegisterCommandHandler(
            userCreateRepositoryMock.Object,
            protectedEntityManagerServiceMock.Object);

        var command = new RegisterCommand
        {
            Username = "user",
            Email = "email@test.com",
            Password = "pass",
            FullName = "name",
            PhoneNumber = "phone",
            Gender = "Male"
        };

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None)).ConfigureAwait(true);
    }

#pragma warning restore CRR0035
#pragma warning restore IDE0079
    [Fact(DisplayName = "AUTH_016 - Validate PhoneNumber format (Unit) - Format số điện thoại")]
    public async Task AUTH_016_Register_ValidatePhoneNumber()
    {
        var userReadRepositoryMock = new Mock<IUserReadRepository>();
        var validator = new RegisterCommandValidator(userReadRepositoryMock.Object);

        var validCommand1 = new RegisterCommand { PhoneNumber = "0912345678" };
        var result1 = await validator.TestValidateAsync(validCommand1, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);
        result1.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);

        var validCommand2 = new RegisterCommand { PhoneNumber = "84912345678" };
        var result2 = await validator.TestValidateAsync(validCommand2, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);
        result2.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);

        var validCommand3 = new RegisterCommand { PhoneNumber = "+84912345678" };
        var result3 = await validator.TestValidateAsync(validCommand3, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);
        result3.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);

        var invalidCommand1 = new RegisterCommand { PhoneNumber = "091234" };
        var resultInv1 = await validator.TestValidateAsync(invalidCommand1, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);
        resultInv1.ShouldHaveValidationErrorFor(x => x.PhoneNumber).WithErrorMessage("Invalid phone number.");

        var invalidCommand2 = new RegisterCommand { PhoneNumber = "abcd123456" };
        var resultInv2 = await validator.TestValidateAsync(invalidCommand2, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);
        resultInv2.ShouldHaveValidationErrorFor(x => x.PhoneNumber).WithErrorMessage("Invalid phone number.");
    }
}
