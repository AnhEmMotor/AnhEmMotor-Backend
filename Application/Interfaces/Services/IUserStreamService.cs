namespace Application.Interfaces.Services;

public interface IUserStreamService
{
    Task WaitForUpdateAsync(Guid userId, CancellationToken cancellationToken);
    void NotifyUserUpdate(Guid userId);
}
