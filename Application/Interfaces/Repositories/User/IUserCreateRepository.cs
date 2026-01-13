using Domain.Entities;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserCreateRepository
    {
        /// <summary>
        /// Creates a new user with the specified password
        /// </summary>
        public Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(
            ApplicationUser user,
            string password,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a user to a role
        /// </summary>
        public Task<(bool Succeeded, IEnumerable<string> Errors)> AddUserToRoleAsync(
            ApplicationUser user,
            string roleName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a user to multiple roles
        /// </summary>
        public Task<(bool Succeeded, IEnumerable<string> Errors)> AddUserToRolesAsync(
            ApplicationUser user,
            IEnumerable<string> roleNames,
            CancellationToken cancellationToken = default);

    }
}
