using Application.ApiContracts.Auth.Requests;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class IdentityService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : IIdentityService
    {
        public async Task<UserAuthDTO> AuthenticateAsync(
            string usernameOrEmail,
            string password,
            CancellationToken cancellationToken)
        {
            ApplicationUser? user;

            cancellationToken.ThrowIfCancellationRequested();

            if(usernameOrEmail.Contains('@'))
            {
                user = await userManager.FindByEmailAsync(usernameOrEmail).ConfigureAwait(false);
            } else
            {
                user = await userManager.FindByNameAsync(usernameOrEmail).ConfigureAwait(false);
            }

            if(user is null)
            {
                throw new UnauthorizedException("Invalid credentials.");
            }

            if(string.Compare(user.Status, UserStatus.Active) != 0 || user.DeletedAt is not null)
            {
                throw new UnauthorizedException("Account is not available.");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false)
                .ConfigureAwait(false);

            if(!result.Succeeded)
            {
                throw new UnauthorizedException("Invalid credentials.");
            }

            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            return new UserAuthDTO()
            {
                Id = user.Id,
                Username = user.UserName,
                Roles = [ .. roles ],
                AuthMethods = [ "amr" ],
                Email = user.Email,
                FullName = user.FullName,
                Status = user.Status
            };
        }
    }
}
