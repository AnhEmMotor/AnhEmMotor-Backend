using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandler(
    IExternalAuthService externalAuthService,
    IIdentityService identityService,
    ITokenManagerService tokenManagerService,
    ICookieTokenManager cookieTokenManager,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<GoogleLoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var externalUserResult = await externalAuthService.ValidateGoogleTokenAsync(request.IdToken, cancellationToken)
            .ConfigureAwait(false);
        if (externalUserResult.IsFailure || externalUserResult.Value is null)
        {
            return externalUserResult.Error ?? Error.Failure("Auth.ExternalError", "External user data is null.");
        }
        var authResult = await identityService.LoginWithExternalProviderAsync(
            externalUserResult.Value,
            cancellationToken)
            .ConfigureAwait(false);
        if (authResult.IsFailure || authResult.Value is null)
        {
            return authResult.Error ?? Error.Failure("Auth.IdentityError", "User record is null.");
        }
        var userDto = authResult.Value;
        var expiryAccessTokenMinutes = tokenManagerService.GetAccessTokenExpiryMinutes();
        var expiryAccessTokenDate = DateTimeOffset.UtcNow.AddMinutes(expiryAccessTokenMinutes);
        var accessToken = tokenManagerService.CreateAccessToken(userDto, expiryAccessTokenDate);
        var refreshToken = tokenManagerService.CreateRefreshToken();
        var expiryRefreshTokenDays = tokenManagerService.GetRefreshTokenExpiryDays();
        var expiryRefreshTokenDate = DateTimeOffset.UtcNow.AddDays(expiryRefreshTokenDays);
        await userUpdateRepository.UpdateRefreshTokenAsync(
            userDto.Id,
            refreshToken,
            expiryRefreshTokenDate,
            cancellationToken)
            .ConfigureAwait(false);
        cookieTokenManager.SetRefreshToken(refreshToken, expiryRefreshTokenDate);
        return new LoginResponse { AccessToken = accessToken, ExpiresAt = expiryAccessTokenDate };
    }
}
