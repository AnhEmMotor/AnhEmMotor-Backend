namespace Application.Features.ManagerChat.Commands.SendPlanChatMessage;

/// <summary>Phân loại ý định duyệt/huỷ kế hoạch từ chat — khớp CHÍNH XÁC toàn bộ câu (không phải
/// substring như SteeringClassifier), vì đây là hành động có hậu quả: không khớp thì rơi xuống
/// nhánh LLM diễn giải (an toàn hơn tự suy đoán "gần đúng").</summary>
public static class PlanChatClassifier
{
    private static readonly HashSet<string> ApproveExact =
        ["duyệt", "đồng ý", "ok", "được", "duyệt đi", "duyệt luôn", "chạy đi", "chạy luôn"];

    private static readonly HashSet<string> RejectExact =
        ["huỷ", "hủy", "không", "thôi", "bỏ", "huỷ đi", "hủy đi", "huỷ luôn", "hủy luôn"];

    public static string? Classify(string text)
    {
        var normalized = text.Trim().TrimEnd('.', '!', '?').ToLowerInvariant();
        if (ApproveExact.Contains(normalized)) return "approve";
        if (RejectExact.Contains(normalized)) return "reject";
        return null;
    }
}
