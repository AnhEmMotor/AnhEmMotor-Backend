using Application.Interfaces.Repositories.User;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repositories.User;

public class UserDeleteRepository(UserManager<ApplicationUser> userManager) : IUserDeleteRepository
{
    public async Task<(bool Succeeded, IEnumerable<string> Errors)> SoftDeleteUserAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.DeletedAt = DateTimeOffset.UtcNow;
        var result = await userManager.UpdateAsync(user).ConfigureAwait(false);

        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> RestoreUserAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.DeletedAt = null;
        var result = await userManager.UpdateAsync(user).ConfigureAwait(false);

        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
}
