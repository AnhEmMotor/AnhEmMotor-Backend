using Domain.Entities;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserDeleteRepository
    {
        /// <summary>
        /// Soft deletes a user by setting DeletedAt timestamp
        /// </summary>
        public Task<(bool Succeeded, IEnumerable<string> Errors)> SoftDeleteUserAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Restores a soft-deleted user by clearing DeletedAt timestamp
        /// </summary>
        public Task<(bool Succeeded, IEnumerable<string> Errors)> RestoreUserAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default);
    }
}
