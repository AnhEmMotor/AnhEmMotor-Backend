using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Features.Auth.Commands.FacebookLogin;
using Application.Features.Auth.Commands.GoogleLogin;
using Application.Features.Auth.Commands.Register;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentValidation.TestHelper;
using Moq;

namespace UnitTests;

public class Auth
{
    private readonly Mock<IExternalAuthService> _externalAuthServiceMock = new();
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<ITokenManagerService> _tokenManagerServiceMock = new();
    private readonly Mock<IHttpTokenAccessorService> _httpTokenAccessorServiceMock = new();
    private readonly Mock<IUserUpdateRepository> _userUpdateRepositoryMock = new();

    private GoogleLoginCommandHandler GetGoogleHandler() => new(
        _externalAuthServiceMock.Object,
        _identityServiceMock.Object,
        _tokenManagerServiceMock.Object,
        _httpTokenAccessorServiceMock.Object,
        _userUpdateRepositoryMock.Object);

    private FacebookLoginCommandHandler GetFacebookHandler() => new(
        _externalAuthServiceMock.Object,
        _identityServiceMock.Object,
        _tokenManagerServiceMock.Object,
        _httpTokenAccessorServiceMock.Object,
        _userUpdateRepositoryMock.Object);

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
        resultInv1.ShouldHaveValidationErrorFor(x => x.PhoneNumber).WithErrorMessage("Invalid phone number format.");
        var invalidCommand2 = new RegisterCommand { PhoneNumber = "abcd123456" };
        var resultInv2 = await validator.TestValidateAsync(invalidCommand2, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);
        resultInv2.ShouldHaveValidationErrorFor(x => x.PhoneNumber).WithErrorMessage("Invalid phone number format.");
    }

    [Fact(DisplayName = "AUTH_017 - Google Login - Thành công (User mới)")]
    public async Task AUTH_017_GoogleLogin_Success_NewUser()
    {
        var command = new GoogleLoginCommand { IdToken = "valid_token" };
        var externalUser = new ExternalUserDto
        {
            Email = "new@test.com",
            Name = "New User",
            Provider = "Google",
            ProviderId = "google_id"
        };
        var userAuth = new UserAuth { Id = Guid.NewGuid(), Email = "new@test.com", FullName = "New User" };
        _externalAuthServiceMock.Setup(
            x => x.ValidateGoogleTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalUserDto>.Success(externalUser));
        _identityServiceMock.Setup(
            x => x.LoginWithExternalProviderAsync(It.IsAny<ExternalUserDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAuth>.Success(userAuth));
        _tokenManagerServiceMock.Setup(x => x.GetAccessTokenExpiryMinutes()).Returns(5);
        _tokenManagerServiceMock.Setup(x => x.CreateAccessToken(It.IsAny<UserAuth>(), It.IsAny<DateTimeOffset>()))
            .Returns("access_token");
        _tokenManagerServiceMock.Setup(x => x.CreateRefreshToken()).Returns("refresh_token");
        _tokenManagerServiceMock.Setup(x => x.GetRefreshTokenExpiryDays()).Returns(7);
        var handler = GetGoogleHandler();
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        Assert.True(result.IsSuccess);
        Assert.Equal("access_token", result.Value.AccessToken);
        _userUpdateRepositoryMock.Verify(
            x => x.UpdateRefreshTokenAsync(
                userAuth.Id,
                "refresh_token",
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "AUTH_018 - Google Login - Thành công (User cũ)")]
    public async Task AUTH_018_GoogleLogin_Success_ExistingUser()
    {
        var command = new GoogleLoginCommand { IdToken = "valid_token" };
        var externalUser = new ExternalUserDto
        {
            Email = "existing@test.com",
            Name = "Existing User",
            Provider = "Google",
            ProviderId = "google_id"
        };
        var userAuth = new UserAuth { Id = Guid.NewGuid(), Email = "existing@test.com", FullName = "Existing User" };
        _externalAuthServiceMock.Setup(
            x => x.ValidateGoogleTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalUserDto>.Success(externalUser));
        _identityServiceMock.Setup(
            x => x.LoginWithExternalProviderAsync(It.IsAny<ExternalUserDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAuth>.Success(userAuth));
        _tokenManagerServiceMock.Setup(x => x.GetAccessTokenExpiryMinutes()).Returns(5);
        _tokenManagerServiceMock.Setup(x => x.CreateAccessToken(It.IsAny<UserAuth>(), It.IsAny<DateTimeOffset>()))
            .Returns("access_token");
        _tokenManagerServiceMock.Setup(x => x.CreateRefreshToken()).Returns("refresh_token");
        _tokenManagerServiceMock.Setup(x => x.GetRefreshTokenExpiryDays()).Returns(7);
        var handler = GetGoogleHandler();
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        Assert.True(result.IsSuccess);
        Assert.Equal("access_token", result.Value.AccessToken);
    }

    [Fact(DisplayName = "AUTH_019 - Google Login - Thất bại (Token giả)")]
    public async Task AUTH_019_GoogleLogin_Failure_InvalidToken()
    {
        var command = new GoogleLoginCommand { IdToken = "invalid_token" };
        _externalAuthServiceMock.Setup(
            x => x.ValidateGoogleTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized("Invalid Google token."));
        var handler = GetGoogleHandler();
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        Assert.True(result.IsFailure);
        Assert.Equal("Unauthorized", result.Error?.Code);
    }

    [Fact(DisplayName = "AUTH_020 - Facebook Login - Thành công")]
    public async Task AUTH_020_FacebookLogin_Success()
    {
        var command = new FacebookLoginCommand { AccessToken = "valid_fb_token" };
        var externalUser = new ExternalUserDto
        {
            Email = "fb@test.com",
            Name = "FB User",
            Provider = "Facebook",
            ProviderId = "fb_id"
        };
        var userAuth = new UserAuth { Id = Guid.NewGuid(), Email = "fb@test.com", FullName = "FB User" };
        _externalAuthServiceMock.Setup(
            x => x.ValidateFacebookTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalUserDto>.Success(externalUser));
        _identityServiceMock.Setup(
            x => x.LoginWithExternalProviderAsync(It.IsAny<ExternalUserDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAuth>.Success(userAuth));
        _tokenManagerServiceMock.Setup(x => x.GetAccessTokenExpiryMinutes()).Returns(5);
        _tokenManagerServiceMock.Setup(x => x.CreateAccessToken(It.IsAny<UserAuth>(), It.IsAny<DateTimeOffset>()))
            .Returns("access_token");
        _tokenManagerServiceMock.Setup(x => x.CreateRefreshToken()).Returns("refresh_token");
        _tokenManagerServiceMock.Setup(x => x.GetRefreshTokenExpiryDays()).Returns(7);
        var handler = GetFacebookHandler();
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        Assert.True(result.IsSuccess);
        Assert.Equal("access_token", result.Value.AccessToken);
    }

    [Fact(DisplayName = "AUTH_021 - Facebook Login - Thất bại")]
    public async Task AUTH_021_FacebookLogin_Failure_InvalidToken()
    {
        var command = new FacebookLoginCommand { AccessToken = "invalid_fb_token" };
        _externalAuthServiceMock.Setup(
            x => x.ValidateFacebookTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized("Invalid Facebook token."));
        var handler = GetFacebookHandler();
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        Assert.True(result.IsFailure);
        Assert.Equal("Unauthorized", result.Error?.Code);
    }
}

