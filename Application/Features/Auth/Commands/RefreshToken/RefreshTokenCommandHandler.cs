using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Mapster;
using MediatR;

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
        var user = await userReadRepository.GetByRefreshTokenAsync(request.RefreshToken!, cancellationToken)
            .ConfigureAwait(false);

        if(user == null)
        {
            return Error.Unauthorized("Invalid refresh token.");
        }

        if(user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            return Error.Unauthorized("Refresh token has expired. Please login again.");
        }

        if(string.Compare(user.Status, "Active") != 0 || user.DeletedAt != null)
        {
            return Error.Forbidden("Account is not available.");
        }

        if(!string.IsNullOrEmpty(request.AccessToken))
        {
            var oldStatusClaim = tokenService.GetClaimFromToken(
                request.AccessToken,
                Domain.Constants.ClaimJWTPayload.Status);

            if(!string.IsNullOrEmpty(oldStatusClaim) &&
                !string.Equals(oldStatusClaim, user.Status, StringComparison.OrdinalIgnoreCase))
            {
                return Error.Unauthorized("User status has changed. Please login again.");
            }
        }

        var userDto = user.Adapt<UserAuth>();
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