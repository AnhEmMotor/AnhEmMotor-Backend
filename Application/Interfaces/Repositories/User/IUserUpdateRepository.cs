using Domain.Entities;
using System;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserUpdateRepository
    {
        public Task UpdateRefreshTokenAsync(
            Guid userId,
            string refreshToken,
            DateTimeOffset expiryTime,
            CancellationToken cancellationToken);

        public Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default);

        public Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(
            ApplicationUser user,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken = default);

        public Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(
            ApplicationUser user,
            string newPassword,
            CancellationToken cancellationToken = default);

        public Task ClearRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);

        public Task<(bool Succeeded, IEnumerable<string> Errors)> RemoveUserFromRolesAsync(
            ApplicationUser user,
            IEnumerable<string> roleNames,
            CancellationToken cancellationToken = default);
    }
}
