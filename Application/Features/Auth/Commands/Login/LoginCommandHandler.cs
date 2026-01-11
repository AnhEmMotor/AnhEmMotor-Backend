using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IIdentityService identityService,
    ITokenManagerService tokenManagerService,
    IHttpTokenAccessorService httpTokenAccessorService,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var authResult = await identityService.AuthenticateAsync(
            request.UsernameOrEmail,
            request.Password,
            cancellationToken)
            .ConfigureAwait(false);

        if(authResult.IsFailure)
        {
            return authResult.Error!;
        }

        var userDto = authResult.Value;

        var expiryAccessTokenMinutes = tokenManagerService.GetAccessTokenExpiryMinutes();
        var expiryAccessTokenDate = DateTimeOffset.UtcNow.AddMinutes(expiryAccessTokenMinutes);
        var accessToken = await tokenManagerService.CreateAccessTokenAsync(
            userDto,
            expiryAccessTokenDate,
            cancellationToken)
            .ConfigureAwait(false);

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