namespace Application.Interfaces.Services;

public interface IUserStreamService
{
    public Task WaitForUpdateAsync(Guid userId, CancellationToken cancellationToken);
    public void NotifyUserUpdate(Guid userId);
}
