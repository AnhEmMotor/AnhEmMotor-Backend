using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Auth.Commands.LoginForManager;

public class LoginForManagerCommandHandler(
    IIdentityService identityService,
    ITokenManagerService tokenManagerService,
    ICookieTokenManager cookieTokenManager,
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IRoleReadRepository roleReadRepository) : IRequestHandler<LoginForManagerCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginForManagerCommand request, CancellationToken cancellationToken)
    {
        var authResult = await identityService.AuthenticateAsync(
            request.UsernameOrEmail!,
            request.Password!,
            cancellationToken)
            .ConfigureAwait(false);
        if (authResult.IsFailure)
        {
            return authResult.Error!;
        }
        var userDto = authResult.Value;
        var user = await userReadRepository.FindUserByIdAsync(userDto.Id, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Error.Unauthorized("User not found.");
        }
        var userRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);
        var roleIds = (await roleReadRepository.GetRolesByNameAsync(userRoles, cancellationToken).ConfigureAwait(false))
            .Select(r => r.Id)
            .ToList();
        var hasAnyPermission = await roleReadRepository.HasAnyPermissionAsync(roleIds, cancellationToken)
            .ConfigureAwait(false);
        if (!hasAnyPermission)
        {
            return Error.Forbidden("Access denied. User does not have the required permissions to access this system.");
        }
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
