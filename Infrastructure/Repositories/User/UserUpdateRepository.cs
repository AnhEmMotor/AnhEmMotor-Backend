using Application.Interfaces.Repositories.User;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repositories.User
{
    public class UserUpdateRepository(UserManager<ApplicationUser> userManager) : IUserUpdateRepository
    {
        public async Task UpdateRefreshTokenAsync(
            Guid userId,
            string refreshToken,
            DateTimeOffset expiryTime,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);
            if(user is not null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryTime;
                await userManager.UpdateAsync(user).ConfigureAwait(false);
            }
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await userManager.UpdateAsync(user).ConfigureAwait(false);

            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(
            ApplicationUser user,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword)
                .ConfigureAwait(false);

            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(
            ApplicationUser user,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
            var result = await userManager.ResetPasswordAsync(user, resetToken, newPassword)
                .ConfigureAwait(false);

            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        public async Task ClearRefreshTokenAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);
            if(user is not null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTimeOffset.MinValue;
                await userManager.UpdateAsync(user).ConfigureAwait(false);
            }
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> RemoveUserFromRolesAsync(
            ApplicationUser user,
            IEnumerable<string> roleNames,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await userManager.RemoveFromRolesAsync(user, roleNames).ConfigureAwait(false);

            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }
    }
}
