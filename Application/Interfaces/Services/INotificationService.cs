namespace Application.Interfaces.Services;

public interface INotificationService
{
    Task<string> WaitForNotificationAsync(CancellationToken cancellationToken);
    void NotifyNewBooking(string message);
}
