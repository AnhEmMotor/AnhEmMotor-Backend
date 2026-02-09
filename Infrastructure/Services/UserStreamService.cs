using Application.Interfaces.Services;
using System.Collections.Concurrent;

namespace Infrastructure.Services;

public class UserStreamService : IUserStreamService
{
    // Dictionary to hold active listeners for each user.
    // Key: UserId, Value: List of TaskCompletionSource (one per connection/request)
    private readonly ConcurrentDictionary<Guid, List<TaskCompletionSource>> _listeners = new();

    public async Task WaitForUpdateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Add the listener
        _listeners.AddOrUpdate(
            userId,
            _ => new List<TaskCompletionSource> { tcs },
            (_, list) =>
            {
                lock(list)
                {
                    list.Add(tcs);
                }
                return list;
            });

        // Register cancellation to remove the listener if the client disconnects
        using var registration = cancellationToken.Register(() =>
        {
            RemoveListener(userId, tcs);
            tcs.TrySetCanceled();
        });

        await tcs.Task.ConfigureAwait(false);
    }

    public void NotifyUserUpdate(Guid userId)
    {
        if(_listeners.TryGetValue(userId, out var list))
        {
            List<TaskCompletionSource> snapshot;
            lock(list)
            {
                // Create a snapshot to iterate over, to avoid modification during iteration
                snapshot = list.ToList();
                // Clear the list because we are waking them all up. 
                // They will re-subscribe if they want to wait for the *next* update.
                list.Clear();
            }

            foreach(var tcs in snapshot)
            {
                // Completing the task wakes up the controller loop
                tcs.TrySetResult();
            }
        }
    }

    private void RemoveListener(Guid userId, TaskCompletionSource tcs)
    {
        if(_listeners.TryGetValue(userId, out var list))
        {
            lock(list)
            {
                list.Remove(tcs);
                if(list.Count == 0)
                {
                    // Remove the key if no more listeners to clean up memory
                    // Note: There is a potential race condition here if a new listener is added while we are removing the key.
                    // However, typical usage patterns (client disconnect vs new connection) make this low risk.
                    // For robustness, we can leave the empty list or implement safer removal.
                    // Currently leaving empty list to avoid race in tests.
                     // _listeners.TryRemove(userId, out _);
                }
            }
        }
    }
}
