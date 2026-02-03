using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<Result<UserAuth>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? user;

        if(usernameOrEmail.Contains('@'))
        {
            user = await userManager.FindByEmailAsync(usernameOrEmail).ConfigureAwait(false);
        } else
        {
            user = await userManager.FindByNameAsync(usernameOrEmail).ConfigureAwait(false);
        }

        if(user == null)
        {
            return Error.Unauthorized("Wrong username/email or password.");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false);

        if(!isPasswordValid)
        {
            return Error.Unauthorized("Wrong username/email or password.");
        }

        if (user.Status == UserStatus.Banned)
        {
            return Error.Forbidden("User is banned.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        return new UserAuth
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = [ .. roles ],
            AuthMethods = [ "amr" ],
            Email = user.Email,
            FullName = user.FullName,
            Status = user.Status
        };
    }
}