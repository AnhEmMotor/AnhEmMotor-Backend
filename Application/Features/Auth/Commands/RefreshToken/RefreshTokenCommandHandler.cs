using Application.ApiContracts.Auth.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Authentication;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    ICurrentUserService currentUserService,
    IAsyncQueryableExecuter asyncExecuter,
    IConfiguration configuration) : IRequestHandler<RefreshTokenCommand, GetAccessTokenFromRefreshTokenResponse>
{
    public async Task<GetAccessTokenFromRefreshTokenResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = currentUserService.GetRefreshToken();
        if(string.IsNullOrEmpty(refreshToken))
        {
            throw new UnauthorizedException("Refresh token is missing.");
        }

        var user = await asyncExecuter.FirstOrDefaultAsync(
                userManager.Users,
                u => string.Compare(u.RefreshToken, refreshToken) == 0,
                cancellationToken)
                .ConfigureAwait(false) ??
            throw new UnauthorizedException("Invalid refresh token.");
        if(string.Compare(user.Status, "Active") != 0)
        {
            throw new ForbiddenException("Please login again.");
        }

        if(string.Compare(user.Status, "Active") == 0 && user.DeletedAt != null)
        {
            throw new ForbiddenException("Please login again.");
        }

        if(user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedException("Refresh token has expired. Please login again.");
        }

        var authHeader = currentUserService.GetAuthorizationHeader();
        if(!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..];
            var tokenStatusClaim = tokenService.GetClaimFromToken(token, "status");

            if(!string.IsNullOrEmpty(tokenStatusClaim) && string.Compare(tokenStatusClaim, user.Status) != 0)
            {
                throw new UnauthorizedException("User status has changed. Please login again.");
            }
        }

        var expiryDays = configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays");

        var newAccessToken = await tokenService.CreateAccessTokenAsync(user, [ "pwd" ], cancellationToken)
            .ConfigureAwait(false);
        var newRefreshToken = tokenService.CreateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(expiryDays);
        await userManager.UpdateAsync(user).ConfigureAwait(false);

        currentUserService.SetRefreshToken(newRefreshToken);

        return new GetAccessTokenFromRefreshTokenResponse
        {
            AccessToken = newAccessToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };
    }
}
