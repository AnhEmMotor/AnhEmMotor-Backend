using Domain.Entities;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserCreateRepository
    {
        public Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(
            ApplicationUser user,
            string password,
            CancellationToken cancellationToken = default);

        public Task<(bool Succeeded, IEnumerable<string> Errors)> AddUserToRoleAsync(
            ApplicationUser user,
            string roleName,
            CancellationToken cancellationToken = default);

        public Task<(bool Succeeded, IEnumerable<string> Errors)> AddUserToRolesAsync(
            ApplicationUser user,
            IEnumerable<string> roleNames,
            CancellationToken cancellationToken = default);
    }
}
