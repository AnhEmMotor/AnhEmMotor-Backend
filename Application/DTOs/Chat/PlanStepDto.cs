using System.Text.Json.Serialization;
using Domain.Constants;

namespace Application.DTOs.Chat;

public record PlanStepDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("order")] int Order,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("detail")] string Detail,
    [property: JsonPropertyName("expectedTools")] List<string> ExpectedTools,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("editedByUser")] bool EditedByUser,
    [property: JsonPropertyName("result")] string? Result,
    // Nullable, mặc định null: plan cũ được lưu trước khi có field này deserialize ra null,
    // không phải []. Nơi cần danh sách cụ thể (vd. thêm bình luận) phải tự `?? []`.
    [property: JsonPropertyName("comments")] List<PlanStepCommentDto>? Comments = null)
{
    public static PlanStepDto NewPending(string id, int order, string title, string detail, List<string> expectedTools) =>
        new(id, order, title, detail, expectedTools, PlanStepStatus.Pending, false, null);
}
