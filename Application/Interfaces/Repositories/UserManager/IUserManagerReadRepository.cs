using Domain.Entities;

namespace Application.Interfaces.Repositories.UserManager;

public interface IUserManagerReadRepository
{
    public Task<bool> IsUsernameAvailableAsync(string username, Guid? excludeUserId);

    public Task<bool> IsEmailAvailableAsync(string email, Guid? excludeUserId);

    public Task<bool> IsPhoneNumberAvailableAsync(string? phoneNumber, Guid? excludeUserId);

    public Task<bool> ValidateAllUsersExistAsync(List<Guid> userIds);

    public Task<List<ApplicationUser>> GetAllUsersAsync();

    public Task<bool> RoleExistsAsync(
        string roleName,
        CancellationToken cancellationToken = default);
}
