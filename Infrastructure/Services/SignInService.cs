using Application.Interfaces.Repositories.Authentication;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class SignInService(SignInManager<ApplicationUser> signInManager) : ISignInService
{
    public Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure)
    {
        return signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
    }
}
