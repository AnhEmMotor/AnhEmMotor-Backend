using Application.ApiContracts.Auth.Requests;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace UnitTests;

public class Auth
{
    [Fact(DisplayName = "AUTH_REG_004 - Validate Format (Unit) - TH1: Email sai định dạng")]
    public void AUTH_REG_004_1_Register_InvalidEmail()
    {
        // Arrange
        var userReadRepositoryMock = new Mock<IUserReadRepository>();
        var validator = new RegisterCommandValidator(userReadRepositoryMock.Object);

        // TH1: Email invalid
        var command = new RegisterCommand("user", "invalid-email", "Pass", "Full Name", "0123456789", "Male");
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact(DisplayName = "AUTH_REG_004 - Validate Format (Unit) - TH2: Password ngắn")]
    public void AUTH_REG_004_2_Register_PasswordTooShort()
    {
        // Arrange
        var userReadRepositoryMock = new Mock<IUserReadRepository>();
        var validator = new RegisterCommandValidator(userReadRepositoryMock.Object);

        // TH2: Password too short
        var command = new RegisterCommand("user", "test@test.com", "123", "Full Name", "0123456789", "Male");
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact(DisplayName = "AUTH_REG_004 - Validate Format (Unit) - TH3: Username chứa ký tự đặc biệt")]
    public void AUTH_REG_004_3_Register_UsernameSpecialChars()
    {
        // Arrange
        var userReadRepositoryMock = new Mock<IUserReadRepository>();
        var validator = new RegisterCommandValidator(userReadRepositoryMock.Object);

        // TH3: Username special chars
        var command = new RegisterCommand("user@#$", "test@test.com", "Password123!", "Full Name", "0123456789", "Male");
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact(DisplayName = "AUTH_UNI_001 - Xử lý Exception")]
    public async Task AUTH_UNI_001_Exception_Handling()
    {
        // Arrange
        var userCreateRepositoryMock = new Mock<IUserCreateRepository>();
        
        // Simulate Exception
        userCreateRepositoryMock.Setup(x => x.CreateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Connection Failed"));

        var protectedEntityManagerServiceMock = new Mock<IProtectedEntityManagerService>();
        var handler = new RegisterCommandHandler(userCreateRepositoryMock.Object, protectedEntityManagerServiceMock.Object);

        var command = new RegisterCommand("user", "email@test.com", "pass", "name", "phone", "Male");

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}
