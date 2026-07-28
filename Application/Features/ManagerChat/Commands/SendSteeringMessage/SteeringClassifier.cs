using Domain.Constants;

namespace Application.Features.ManagerChat.Commands.SendSteeringMessage;

/// <summary>Phân loại chế độ steering — chỉ Tầng 1 (luật, xem 09-STAGE-STEERING.md mục 9.4).</summary>
public static class SteeringClassifier
{
    private static readonly string[] CorrectionMarkers =
    [
        "à nhầm", "à quên", "nhầm rồi", "sai rồi", "không phải",
        "ý tôi là", "ý mình là", "sửa lại", "đổi thành", "thay vì",
        "khoan", "dừng lại", "bỏ qua",
    ];

    private static readonly HashSet<string> RestartExact = ["dừng", "stop", "thôi", "huỷ"];

    // ponytail: chỉ Tầng 1 (rule-based). Tầng 2 (LLM-classify khi không chắc) chưa làm —
    // không chắc thì handler mặc định về Queue, đúng nguyên tắc an toàn của 9.1.
    // Thêm Tầng 2 khi tỷ lệ "không chắc" đo được đủ cao để cần độ chính xác hơn.
    public static string? Classify(string text)
    {
        var lowered = text.Trim().ToLowerInvariant();
        if (CorrectionMarkers.Any(lowered.Contains)) return ChatSteeringMode.Interrupt;
        if (RestartExact.Contains(lowered)) return ChatSteeringMode.Restart;
        return null;
    }
}
