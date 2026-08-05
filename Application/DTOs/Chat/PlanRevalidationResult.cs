namespace Application.DTOs.Chat;

/// <summary>
/// Kết quả revalidate plan trước khi resume (Stage 17.8) — sidecar là nguồn thật của TOOL_SPECS.
/// </summary>
public record PlanRevalidationResult(bool Ok, List<string> UnavailableTools);
