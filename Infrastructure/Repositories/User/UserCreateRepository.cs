using Application.Interfaces.Repositories.User;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repositories.User;

public class UserCreateRepository(UserManager<ApplicationUser> userManager) : IUserCreateRepository
{
    public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(
        ApplicationUser user,
        string password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await userManager.CreateAsync(user, password).ConfigureAwait(false);

        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> AddUserToRoleAsync(
        ApplicationUser user,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await userManager.AddToRoleAsync(user, roleName).ConfigureAwait(false);

        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> AddUserToRolesAsync(
        ApplicationUser user,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await userManager.AddToRolesAsync(user, roleNames).ConfigureAwait(false);

        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
}
