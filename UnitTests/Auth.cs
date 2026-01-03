using Application.ApiContracts.Auth.Requests;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
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
    [Fact]
    public void AUTH_REG_004_1_Register_InvalidEmail()
    {
        // Arrange
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        var validator = new RegisterCommandValidator(userManagerMock.Object);

        // TH1: Email invalid
        var command = new RegisterCommand("user", "invalid-email", "Pass", "Full Name", "0123456789", "Male");
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void AUTH_REG_004_2_Register_PasswordTooShort()
    {
        // Arrange
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        var validator = new RegisterCommandValidator(userManagerMock.Object);

        // TH2: Password too short
        var command = new RegisterCommand("user", "test@test.com", "123", "Full Name", "0123456789", "Male");
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void AUTH_REG_004_3_Register_UsernameSpecialChars()
    {
        // Arrange
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        var validator = new RegisterCommandValidator(userManagerMock.Object);

        // TH3: Username special chars
        var command = new RegisterCommand("user@#$", "test@test.com", "Password123!", "Full Name", "0123456789", "Male");
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public async Task AUTH_UNI_001_Exception_Handling()
    {
        // Arrange
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        
        // Simulate Exception
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("DB Connection Failed"));

        var handler = new RegisterCommandHandler(userManagerMock.Object, null); // null for other deps

        var command = new RegisterCommand("user", "email@test.com", "pass", "name", "phone", "Male");

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}
