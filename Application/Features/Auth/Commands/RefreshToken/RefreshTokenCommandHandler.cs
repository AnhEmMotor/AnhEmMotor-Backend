using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;
using Mapster; 

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    ITokenManagerService tokenService,
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IHttpTokenAccessorService httpTokenAccessor) : IRequestHandler<RefreshTokenCommand, Result<GetAccessTokenFromRefreshTokenResponse>>
{
    public async Task<Result<GetAccessTokenFromRefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var currentRefreshToken = httpTokenAccessor.GetRefreshTokenFromCookie();
        if (string.IsNullOrEmpty(currentRefreshToken))
        {
            return Error.Unauthorized("Refresh token is missing.");
        }

        var user = await userReadRepository.GetByRefreshTokenAsync(currentRefreshToken, cancellationToken)
            .ConfigureAwait(false);

        if (user == null)
        {
            return Error.Unauthorized("Invalid refresh token.");
        }

        if (user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            return Error.Unauthorized("Refresh token has expired. Please login again.");
        }

        if (user.Status != "Active" || user.DeletedAt != null)
        {
            return Error.Forbidden("Account is not available.");
        }

        var authHeader = httpTokenAccessor.GetAuthorizationValueFromHeader();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var oldAccessToken = authHeader["Bearer ".Length..];
            var oldStatusClaim = tokenService.GetClaimFromToken(oldAccessToken, ClaimConstants.Status);

            if (!string.IsNullOrEmpty(oldStatusClaim) &&
                !string.Equals(oldStatusClaim, user.Status, StringComparison.OrdinalIgnoreCase))
            {
                return Error.Unauthorized("User status has changed. Please login again.");
            }
        }

        var userDto = user.Adapt<UserAuthDTO>();

        var accessExpiryMinutes = tokenService.GetAccessTokenExpiryMinutes();
        var refreshExpiryDays = tokenService.GetRefreshTokenExpiryDays();

        var now = DateTimeOffset.UtcNow;
        var accessTokenExpiresAt = now.AddMinutes(accessExpiryMinutes);
        var refreshTokenExpiresAt = now.AddDays(refreshExpiryDays);

        var newAccessToken = tokenService.CreateAccessToken(userDto, accessTokenExpiresAt);
            
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