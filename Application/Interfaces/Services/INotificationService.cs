
namespace Application.Interfaces.Services;

public interface INotificationService
{
    public Task<string> WaitForNotificationAsync(CancellationToken cancellationToken);

    public void NotifyNewBooking(string message);
}
