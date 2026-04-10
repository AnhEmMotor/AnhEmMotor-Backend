using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Auth.Commands.FacebookLogin;

public class FacebookLoginCommandHandler(
    IExternalAuthService externalAuthService,
    IIdentityService identityService,
    ITokenManagerService tokenManagerService,
    IHttpTokenAccessorService httpTokenAccessorService,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<FacebookLoginCommand, Result<LoginResponse>>
{
    private readonly IExternalAuthService externalAuthService = externalAuthService ??
        throw new ArgumentNullException(nameof(externalAuthService));
    private readonly IIdentityService identityService = identityService ??
        throw new ArgumentNullException(nameof(identityService));
    private readonly ITokenManagerService tokenManagerService = tokenManagerService ??
        throw new ArgumentNullException(nameof(tokenManagerService));
    private readonly IHttpTokenAccessorService httpTokenAccessorService = httpTokenAccessorService ??
        throw new ArgumentNullException(nameof(httpTokenAccessorService));
    private readonly IUserUpdateRepository userUpdateRepository = userUpdateRepository ??
        throw new ArgumentNullException(nameof(userUpdateRepository));

    public async Task<Result<LoginResponse>> Handle(FacebookLoginCommand request, CancellationToken cancellationToken)
    {
        var externalUserResult = await externalAuthService.ValidateFacebookTokenAsync(
            request.AccessToken,
            cancellationToken)
            .ConfigureAwait(false);

        if(externalUserResult.IsFailure || externalUserResult.Value is null)
        {
            return externalUserResult.Error ?? Error.Failure("Auth.ExternalError", "External user data is null.");
        }

        var authResult = await identityService.LoginWithExternalProviderAsync(
            externalUserResult.Value,
            cancellationToken)
            .ConfigureAwait(false);

        if(authResult.IsFailure || authResult.Value is null)
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

        httpTokenAccessorService.SetRefreshTokenToCookie(refreshToken, expiryRefreshTokenDate);

        return new LoginResponse { AccessToken = accessToken, ExpiresAt = expiryAccessTokenDate };
    }
}