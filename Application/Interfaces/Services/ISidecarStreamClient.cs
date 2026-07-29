using Application.DTOs.Chat;

namespace Application.Interfaces.Services;

public interface ISidecarStreamClient
{
    public IAsyncEnumerable<SidecarEvent> StreamAsync(Guid runId, Guid sessionId, string message,
        string token, CancellationToken ct);
    public Task CancelAsync(Guid runId, CancellationToken ct = default);
}
