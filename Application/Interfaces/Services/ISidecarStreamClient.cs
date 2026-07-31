using Application.DTOs.Chat;

namespace Application.Interfaces.Services;

public interface ISidecarStreamClient
{
    public IAsyncEnumerable<SidecarEvent> StreamAsync(Guid runId, Guid sessionId, string message,
        string token, CancellationToken ct);
    public Task CancelAsync(Guid runId, CancellationToken ct = default);

    /// <summary>Kiểm tra registry_fingerprint hiện tại của sidecar trước khi resume (Stage 17.8).</summary>
    public Task<PlanRevalidationResult> RevalidatePlanAsync(
        Guid runId, IReadOnlyList<string> expectedTools, string? fingerprint, CancellationToken ct = default);
}
