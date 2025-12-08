using Application.ApiContracts.Auth.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IIdentityService identityService,
    ITokenManagerService tokenManagerService,
    IHttpTokenAccessorService httpTokenAccessorService, IUserUpdateRepository userUpdateRepository) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userDto = await identityService.AuthenticateAsync(request.UsernameOrEmail, request.Password);

        var expiryAccessTokenMinutes = tokenManagerService.GetAccessTokenExpiryMinutes();
        var expiryAccessTokenDate = DateTimeOffset.UtcNow.AddMinutes(expiryAccessTokenMinutes);
        var accessToken = await tokenManagerService.CreateAccessTokenAsync(userDto, expiryAccessTokenDate, cancellationToken);
        
        var refreshToken = tokenManagerService.CreateRefreshToken();
        var expiryRefreshTokenDays = tokenManagerService.GetRefreshTokenExpiryDays();
        var expiryRefreshTokenDate = DateTimeOffset.UtcNow.AddDays(expiryRefreshTokenDays);
        await userUpdateRepository.UpdateRefreshTokenAsync(userDto.Id, refreshToken, expiryRefreshTokenDate);
        httpTokenAccessorService.SetRefreshTokenToCookie(refreshToken, expiryRefreshTokenDate);

        return new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expiryAccessTokenDate
        };
    }
}