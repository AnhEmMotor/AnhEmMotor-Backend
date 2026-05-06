using Application.Interfaces.Services;
using System.Collections.Concurrent;

namespace Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ConcurrentBag<TaskCompletionSource<string>> _listeners = new();

    public async Task<string> WaitForNotificationAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _listeners.Add(tcs);
        try
        {
            return await tcs.Task.WaitAsync(cancellationToken);
        } finally
        {
        }
    }

    public void NotifyNewBooking(string message)
    {
        var snapshot = _listeners.ToArray();
        while (_listeners.TryTake(out _))
        {
        }
        foreach (var tcs in snapshot)
        {
            tcs.TrySetResult(message);
        }
    }
}
