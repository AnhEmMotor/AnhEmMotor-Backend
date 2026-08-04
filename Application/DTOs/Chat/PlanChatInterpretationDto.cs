using System.Text.Json.Serialization;

namespace Application.DTOs.Chat;

/// <summary>Kết quả sidecar diễn giải 1 tin nhắn chat tự do thành thao tác sửa plan (Stage 10.9,
/// endpoint sidecar POST /plan/interpret).</summary>
public record PlanChatInterpretationDto(string Intent, List<PlanChatInterpretedOperationDto> Operations, string Reply);

public record PlanChatInterpretedOperationDto(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("step_id")] string? StepId,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("detail")] string? Detail,
    [property: JsonPropertyName("comment")] string? Comment,
    [property: JsonPropertyName("order")] int? Order,
    [property: JsonPropertyName("expected_tools")] List<string>? ExpectedTools);
