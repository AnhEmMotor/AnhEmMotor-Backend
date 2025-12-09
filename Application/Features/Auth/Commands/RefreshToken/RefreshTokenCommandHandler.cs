using Application.ApiContracts.Auth.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    ITokenManagerService tokenService,
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IHttpTokenAccessorService httpTokenAccessor) : IRequestHandler<RefreshTokenCommand, GetAccessTokenFromRefreshTokenResponse>
{
    public async Task<GetAccessTokenFromRefreshTokenResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var currentRefreshToken = httpTokenAccessor.GetRefreshTokenFromCookie();
        if(string.IsNullOrEmpty(currentRefreshToken))
        {
            throw new UnauthorizedException("Refresh token is missing.");
        }

        var user = await userReadRepository.GetUserByRefreshTokenAsync(currentRefreshToken, cancellationToken)
            .ConfigureAwait(false);

        var authHeader = httpTokenAccessor.GetAuthorizationValueFromHeader();
        if(!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var oldAccessToken = authHeader["Bearer ".Length..];
            var oldStatusClaim = tokenService.GetClaimFromToken(oldAccessToken, "status");

            if(!string.IsNullOrEmpty(oldStatusClaim) &&
                !string.Equals(oldStatusClaim, user.Status, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedException("User status has changed. Please login again.");
            }
        }

        var accessExpiryMinutes = tokenService.GetAccessTokenExpiryMinutes();
        var refreshExpiryDays = tokenService.GetRefreshTokenExpiryDays();

        var now = DateTimeOffset.UtcNow;
        var accessTokenExpiresAt = now.AddMinutes(accessExpiryMinutes);
        var refreshTokenExpiresAt = now.AddDays(refreshExpiryDays);

        var newAccessToken = await tokenService.CreateAccessTokenAsync(user, accessTokenExpiresAt, cancellationToken)
            .ConfigureAwait(false);
        var newRefreshToken = tokenService.CreateRefreshToken();

        await userUpdateRepository.UpdateRefreshTokenAsync(
            user.Id,
            newRefreshToken,
            refreshTokenExpiresAt,
            cancellationToken)
            .ConfigureAwait(false);

        httpTokenAccessor.SetRefreshTokenToCookie(newRefreshToken, refreshTokenExpiresAt);

        return new GetAccessTokenFromRefreshTokenResponse
        {
            AccessToken = newAccessToken,
            ExpiresAt = accessTokenExpiresAt
        };
    }
}