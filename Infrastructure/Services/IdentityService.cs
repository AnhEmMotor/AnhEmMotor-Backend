using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IIdentityService
{
    public async Task<Result<UserAuth>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user;


        if(usernameOrEmail.Contains('@'))
        {
            user = await userManager.FindByEmailAsync(usernameOrEmail).ConfigureAwait(false);
        }
        else
        {
            user = await userManager.FindByNameAsync(usernameOrEmail).ConfigureAwait(false);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user!, password, lockoutOnFailure: false)
            .ConfigureAwait(false);

        var roles = await userManager.GetRolesAsync(user!).ConfigureAwait(false);

        return new UserAuth
        {
            Id = user!.Id,
                UserName = user.UserName,
                Roles = [ .. roles ],
                AuthMethods = [ "amr" ],
                Email = user.Email,
                FullName = user.FullName,
                Status = user.Status
        };
    }
}
