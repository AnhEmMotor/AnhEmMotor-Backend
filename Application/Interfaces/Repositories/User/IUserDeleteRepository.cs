using Domain.Entities;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserDeleteRepository
    {
        public Task<(bool Succeeded, IEnumerable<string> Errors)> SoftDeleteUserAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default);

        public Task<(bool Succeeded, IEnumerable<string> Errors)> RestoreUserAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default);
    }
}
