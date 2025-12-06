using Application.ApiContracts.Auth.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.Authentication;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    ISignInService signInService,
    ITokenService tokenService,
    ICurrentUserService currentUserService,
    IConfiguration configuration) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        ApplicationUser? user;

        if (request.UsernameOrEmail.Contains('@'))
        {
            user = await userManager.FindByEmailAsync(request.UsernameOrEmail);
        }
        else
        {
            user = await userManager.FindByNameAsync(request.UsernameOrEmail);
        }

        if (user is null)
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (user.Status != UserStatus.Active || user.DeletedAt is not null)
        {
            throw new UnauthorizedException("Account is not available.");
        }

        var result = await signInService.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        var expiryDays = configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays");

        var accessToken = await tokenService.CreateAccessTokenAsync(user, ["pwd"]);
        var refreshToken = tokenService.CreateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(expiryDays);
        await userManager.UpdateAsync(user);

        currentUserService.SetRefreshToken(refreshToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };
    }
}
