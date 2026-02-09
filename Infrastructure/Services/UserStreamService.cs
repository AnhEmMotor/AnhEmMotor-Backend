using Application.Interfaces.Services;
using System.Collections.Concurrent;

namespace Infrastructure.Services;

public class UserStreamService : IUserStreamService
{
    private readonly ConcurrentDictionary<Guid, List<TaskCompletionSource>> _listeners = new();

    public async Task WaitForUpdateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _listeners.AddOrUpdate(
            userId,
            _ => [ tcs ],
            (_, list) =>
            {
                lock(list)
                {
                    list.Add(tcs);
                }
                return list;
            });

        try
        {
            await tcs.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        } finally
        {
            RemoveListener(userId, tcs);
        }
    }

    public void NotifyUserUpdate(Guid userId)
    {
        if(_listeners.TryGetValue(userId, out var list))
        {
            List<TaskCompletionSource> snapshot;
            lock(list)
            {
                snapshot = [ .. list ];
                list.Clear();
            }

            foreach(var tcs in snapshot)
            {
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
                }
            }
        }
    }
}
