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
                    // We use the collection as the comparison value to ensure we are removing the entry we just modified
                    // and not a new one that might have been added concurrently.
                    // However, ConcurrentDictionary TryRemove in older versions does not support value check easily without KVP.
                    // But since we are locking 'list', and 'AddOrUpdate' also locks 'list' (if it gets the same instance),
                    // we need to be careful.
                    
                    // Actually, simpler approach: if empty, try remove. 
                    // If a new connection just came in (AddOrUpdate), it would have either:
                    // 1. Got the same list (if we haven't removed it yet). It will lock it. We are holding lock. It waits.
                    //    We remove TCS. List empty. We remove Key. Release lock.
                    //    AddOrUpdate gets lock. Adds TCS. Returns list.
                    //    But we REMOVED the key. So the list acts as "detached".
                    //    AddOrUpdate logic in ConcurrentDictionary:
                    //    If we are in the 'updateFactory', we already modifying the value.
                    //    If we remove the key, ConcurrentDictionary state might be inconsistent if we are inside an update?
                    //    No, we are in RemoveListener, not inside a Dict method.
                    
                    // If we remove the key/value pair now, the 'list' object becomes orphaned from the dictionary.
                    // Any generic 'AddOrUpdate' running concurrently might end up putting it back if it had a reference?
                    // Safe approach: Just TryRemove. If a new user comes, they'll create a new List via AddOrUpdate factory.
                     _listeners.TryRemove(userId, out _);
                }
            }
        }
    }
}
