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

    /// <summary>Diễn giải 1 tin nhắn chat tự do thành thao tác sửa plan (Stage 10.9).</summary>
    public Task<PlanChatInterpretationDto> InterpretPlanChatAsync(
        Guid runId, string message, List<PlanStepDto> steps, string? targetStepId, CancellationToken ct = default);
}
