using System.Text.Json;

namespace Application.DTOs.Chat;

/// <summary>Bước suy nghĩ/gọi tool của 1 tin nhắn AI, tái dựng từ ChatRunEvent để hiển thị lại khi
/// mở lịch sử chat cũ — cùng hình dạng với ChatReasoningStep phía FE (kind: "thinking" | "tool").</summary>
public record ChatReasoningStepDto(
    string Kind,
    string? Text = null,
    string? Name = null,
    string? Label = null,
    string? Summary = null,
    string? Status = null,
    int? DurationMs = null,
    JsonElement? ArgsPreview = null,
    JsonElement? ResultPreview = null,
    bool? Truncated = null,
    int? TotalCount = null,
    string? AsOf = null,
    List<string>? Warnings = null,
    Dictionary<string, string>? FiltersApplied = null
);
