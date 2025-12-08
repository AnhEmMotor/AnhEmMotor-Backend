using Application.ApiContracts.Auth.Requests;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class IdentityService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : IIdentityService
    {
        public async Task<UserAuthDTO> AuthenticateAsync(string usernameOrEmail, string password)
        {
            ApplicationUser? user;

            // Logic check @ move về đây
            if (usernameOrEmail.Contains('@'))
            {
                user = await userManager.FindByEmailAsync(usernameOrEmail);
            }
            else
            {
                user = await userManager.FindByNameAsync(usernameOrEmail);
            }

            if (user is null)
            {
                throw new UnauthorizedException("Invalid credentials.");
            }

            // Logic check status move về đây
            if (user.Status != UserStatus.Active || user.DeletedAt is not null)
            {
                throw new UnauthorizedException("Account is not available.");
            }

            // Logic check pass move về đây
            var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                throw new UnauthorizedException("Invalid credentials.");
            }

            // Mapping từ Entity sang DTO
            var roles = await userManager.GetRolesAsync(user);
            return new UserAuthDTO() { Id = user.Id, Username = user.UserName, Roles = [.. roles], AuthMethods = ["amr"], Email = user.Email, FullName = user.FullName, Status = user.Status };
        }
    }
}
